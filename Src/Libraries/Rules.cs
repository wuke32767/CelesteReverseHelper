using Celeste.Mod.ReverseHelper.Libraries;
using Mono.Cecil;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using OpCode = Mono.Cecil.Cil.OpCode;
using OpCodes = Mono.Cecil.Cil.OpCodes;

#pragma warning disable CL0006
#pragma warning disable CL0005

namespace MonoMod
{
    /// <summary>
    /// transform EmitStaticLambda.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
#if !DEBUG
    [MonoModCustomAttribute(nameof(MonoModRules.Optimization))]
#endif
    public class MakeFasterAttribute : Attribute;

    /// <summary>
    /// track an private, non-clscompliant, nested class field.
    /// </summary>
    /// <param name="__">
    /// class path. start from a type, then strings.
    /// if first one is string, it is assembly name.
    /// </param>
    [AttributeUsage(AttributeTargets.Method)]
    [MonoModCustomMethodAttribute(nameof(MonoModRules.GetField))]
    public class FieldAccessorAttribute(params object[] __) : Attribute;

    /// <summary>
    /// for primary constructor, insert this function there.
    /// does not work for classes that can't be written in non-primary constructor styled.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [MonoModCustomAttribute(nameof(MonoModRules.Then))]
    public class PostInitializeAttribute() : Attribute;

    /// <summary>
    /// assume all rec calls are tail calls and rewrite them into loop.
    /// does not check.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [MonoModCustomAttribute(nameof(MonoModRules.LoopLoop))]
    public class ForceTailRecAttribute() : Attribute;

    /// <summary>
    /// defer a method throw to object creation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
#if !DEBUG
    [MonoModCustomAttribute(nameof(MonoModRules.Later))]
#endif
    public class ThrowDeferredAttribute() : Attribute;

    public static partial class MonoModRules
    {
        public static MethodDefinition Migrate(this MethodDefinition o, MethodDefinition? c = null)
        {
            if (c is null)
                c = new MethodDefinition(o.Name, o.Attributes, o.ReturnType);
            c.Name = o.Name;
            c.Attributes = o.Attributes;
            c.ReturnType = o.ReturnType;
            c.DeclaringType = o.DeclaringType;
            c.MetadataToken = c.MetadataToken;
            c.Body = o.Body;
            c.DebugInformation = o.DebugInformation;
            o.Body = new(o);
            o.DebugInformation = null;
            c.Attributes = o.Attributes;
            c.ImplAttributes = o.ImplAttributes;
            c.PInvokeInfo = o.PInvokeInfo;
            c.IsPreserveSig = o.IsPreserveSig;
            c.IsPInvokeImpl = o.IsPInvokeImpl;

            foreach (var genParam in o.GenericParameters)
                c.GenericParameters.Add(genParam.Clone());

            foreach (var param in o.Parameters)
                c.Parameters.Add(param.Clone());

            foreach (var @override in o.Overrides)
                c.Overrides.Add(@override);
            o.Overrides.Clear();

            if (c.Body != null)
            {
                int foundIndex;
                foreach (var ci in c.Body.Instructions)
                {
                    if (ci.Operand is GenericParameter genParam &&
                        (foundIndex = o.GenericParameters.IndexOf(genParam)) != -1)
                    {
                        ci.Operand = c.GenericParameters[foundIndex];
                    }
                    else if (ci.Operand is ParameterDefinition param &&
                        (foundIndex = o.Parameters.IndexOf(param)) != -1)
                    {
                        ci.Operand = c.Parameters[foundIndex];
                    }
                }
            }

            return c;
        }

        public static void Later(MethodDefinition method, CustomAttribute __)
        {
            var m = method.Migrate();
            m.Name = $"Impl<{method.Name}>";
            var module = method.Module;
            var exctype = module.ImportReference(typeof(Exception));
            var name = "@exception has been deferred";
            var failed = method.DeclaringType.FindField(name);
            if (failed is null)
            {
                failed = new FieldDefinition(name, FieldAttributes.Static, exctype);
                method.DeclaringType.Fields.Add(failed);
                var fixup = new MethodDefinition("@check exception", MethodAttributes.Static, module.TypeSystem.Void);
                using ILContext il = new(fixup);
                method.DeclaringType.Methods.Add(fixup);
                il.Invoke(context =>
                {
                    var ic = new ILCursor(context);
                    ic.EmitLdsfld(failed);
                    var l = ic.DefineLabel();
                    ic.EmitBrfalse(l);
                    ic.EmitLdsfld(failed);
                    ic.EmitDelegate(ExceptionDispatchInfo.Throw);
                    ic.MarkLabel(l);
                    ic.EmitRet();
                });
                foreach (var mx in method.DeclaringType.Methods)
                {
                    if (mx.Name == ".ctor")
                    {
                        mx.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, fixup));
                    }
                }
            }

            method.DeclaringType.Methods.Add(m);
            using ILContext context = new(method);
            context.Invoke(context =>
            {
                var ic = new ILCursor(context);
                var ex = new ExceptionHandler(ExceptionHandlerType.Catch) { CatchType = exctype };
                method.Body.ExceptionHandlers.Add(ex);
                if (!method.IsStatic)
                {
                    ic.EmitLdarg0();
                }

                foreach (var p in method.Parameters)
                {
                    ic.EmitLdarg(p);
                }

                ex.TryStart = ic.Instrs[0];
                ic.EmitCall(m);
                var l = ic.DefineLabel();
                ic.EmitLeave(l);
                ic.EmitStsfld(failed);
                ex.HandlerStart = ex.TryEnd = ic.Prev;
                ic.EmitLeave(l);
                ic.MarkLabel(l);
                ic.EmitRet();
                ex.HandlerEnd = ic.Prev;
            });
        }

        public static void Optimization(MethodDefinition method, CustomAttribute __)
        {
            using ILContext context = new(method);
            context.Invoke(context =>
            {
                //Debugger.Launch();
                var ic = new ILCursor(context);
                HashSet<FieldDefinition> f = [];
                FieldReference? def = null;
                MethodReference? tar = null;
                string? str = null;
                while (ic.TryGotoNext(MoveType.Before,
                        i => i.MatchDup(),
                        i => i.MatchBrtrue(out _),
                        i => i.MatchPop(),
                        i => i.MatchLdnull() || i.MatchLdsfld(out _),
                        i => i.MatchLdftn(out tar) && tar.DeclaringType is TypeDefinition def &&
                            def.Name.StartsWith("<"),
                        i => i.MatchNewobj(out _),
                        i => i.MatchDup(),
                        i => i.MatchStsfld(out def),
                        i => i.MatchLdstr(out str) || i.MatchLdnull(),
                        i => i.MatchCallOrCallvirt(out var def)
                            && def.DeclaringType is TypeDefinition deff
                            && def.Name == nameof(ReflectionExt.EmitStaticLambda)
                            && deff.Name == nameof(ReflectionExt))
                    && def!.DeclaringType is TypeDefinition deff)
                {
                    var me = tar!;
                    ic.Next!.OpCode = OpCodes.Pop;
                    ic.Index++;
                    var x = ic.Next.Offset;
                    ic.RemoveRange(8);
                    var y = ic.Next.Offset;
                    for (int i = 0; i < y - x - 5; i++)
                    {
                        ic.EmitNop();
                    }

                    ic.MoveAfterLabels();
                    ic.EmitLdtoken(me!);
                    //def.Resolve();
                    //ic.Next = ic.Next!.Next; 
                    var k = (MethodReference)ic.Next!.Operand!;
                    var ext = (TypeDefinition)k.DeclaringType;
                    k = ext.FindMethod(nameof(ReflectionExt.EmitStaticLambdaReallyQuick));
                    ic.Next.Operand = k;

                    var me2 = me.Resolve()!;
                    var mytype = (TypeDefinition)me.DeclaringType;
                    me2.Name = str ?? me2.Name;
                    ic.Prev.Operand = me2;
                    var processor = me2.Body.GetILProcessor();
                    me2.IsStatic = true;
                    me2.HasThis = false;
                    mytype.Methods.Remove(me2);
                    mytype.DeclaringType.Methods.Add(me2);
                    foreach (var instr in processor.Body.Instructions)
                    {
                        if (instr.OpCode == OpCodes.Ldarg_0)
                        {
                            throw new Exception("huh");
                        }
                        else if (instr.OpCode == OpCodes.Ldarg_1)
                        {
                            instr.OpCode = OpCodes.Ldarg_0;
                        }
                        else if (instr.OpCode == OpCodes.Ldarg_2)
                        {
                            instr.OpCode = OpCodes.Ldarg_1;
                        }
                        else if (instr.OpCode == OpCodes.Ldarg_3)
                        {
                            instr.OpCode = OpCodes.Ldarg_2;
                        }
                    }
                }
                //ic.Index = 0;
                //while (ic.TryGotoNext(MoveType.Before, i => i.MatchNewobj(out var t) && t.Resolve().DeclaringType.BaseType.Is(typeof(MulticastDelegate))))
                //{
                //    ic.Index--;
                //    ic.EmitPop();
                //    var me = ic.Next?.Operand as MethodDefinition;
                //    if (me is { } && me.DeclaringType.Name.StartsWith("<"))
                //    {
                //    }
                //    ic.Index += 2;
                //}
            });
        }

        public static void Then(MethodDefinition method, CustomAttribute __)
        {
            var ctor = method.DeclaringType.Methods.First(a => a.Name == ".ctor");
            do
            {
                using ILContext context = new(ctor);
                ILCursor ic = new(context);
                MethodReference def = null!;
                if (ic.TryGotoNext(i => i.MatchCallOrCallvirt(out def!) && def.Name == ".ctor")
                    && def.SafeResolve() is { } another && another.DeclaringType == method.DeclaringType)
                {
                    ctor = another;
                    continue;
                }

                ic.Index = 0;
                while (ic.TryGotoNext(MoveType.Before, i => i.MatchRet()))
                {
                    ic.EmitLdarg0();
                    foreach (var v in method.Parameters)
                    {
                        ic.EmitLdarg(ctor.Parameters.First(a => a.Name == v.Name));
                    }

                    ic.EmitCall(method);
                    ic.Index++;
                }

                break;
            } while (true);
        }

        public static void LoopLoop(MethodDefinition method, CustomAttribute __)
        {
            using ILContext context = new(method);
            context.Invoke(context =>
            {
                ILCursor ic = new(context);
                var lb = ic.MarkLabel();

                ic.Index = 0;
                while (ic.TryGotoNext(MoveType.AfterLabel,
                    i => i.MatchCallOrCallvirt(out var def) && def.SafeResolve() == method))
                {
                    ic.Remove();
                    foreach (var v in method.Parameters.Reverse())
                    {
                        ic.EmitStarg(v);
                    }

                    if (!method.IsStatic)
                    {
                        ic.EmitStarg(0);
                    }

                    ic.EmitBr(lb);
                    ic.Index++;
                }
            });
        }

        public static void GetField(ILContext il, CustomAttribute attr)
        {
            //Debugger.Launch();
            var _str = attr.ConstructorArguments[0].Value as CustomAttributeArgument[];
            var str = _str!.Select(x => ((CustomAttributeArgument)x.Value).Value).ToArray();
            ILCursor ic = new(il);
            var s = str.AsSpan();
            s = s[..(s.Length - 1)];
            TypeReference? cur = null;
            AssemblyNameReference asm = null!;
            if (s[0] is TypeReference t)
            {
                s = s[1..];
                cur = t;
            }
            else if (s[0] is string @as)
            {
                s = s[1..];
                asm = new AssemblyNameReference(@as, new());
            }

            while (s.Length > 0)
            {
                var type = (s[0] as string)!;
                var ns = type.LastIndexOf('.');

                string? nsx = null;
                string tsx = type;
                if (ns != -1)
                {
                    nsx = type[..ns];
                    tsx = type[(ns + 1)..];
                }

                s = s[1..];
                var self = new TypeReference(nsx, tsx, il.Module, asm);
                if (cur is not null)
                {
                    self.DeclaringType = cur;
                }

                cur = self;
            }

            var f = str[^1];
            var fr = new FieldReference(f as string, il.Method.ReturnType.GetElementType());
            fr.DeclaringType = cur;
            fr = il.Module.ImportReference(fr);
            il.Instrs.Clear();
            if (il.Method.Parameters.Count == 1)
            {
                ic.EmitLdarg0();
                ic.EmitLdflda(fr);
            }
            else
            {
                ic.EmitLdsflda(fr);
            }

            ic.EmitRet();
        }
    }
}
