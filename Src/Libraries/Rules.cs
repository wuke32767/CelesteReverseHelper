using Celeste.Mod.ReverseHelper.Libraries;
using Mono.Cecil;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Security.Cryptography;
using OpCodes = Mono.Cecil.Cil.OpCodes;
#pragma warning disable CL0006
#pragma warning disable CL0005

namespace MonoMod
{
    [MonoModCustomAttribute(nameof(MonoModRules.Optimization))]
    public class MakeFasterAttribute : Attribute;
    [MonoModCustomMethodAttribute(nameof(MonoModRules.GetField))]
    public class FieldAccessorAttribute(params object[] __) : Attribute;
    public class MonoModRules
    {
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
                    i => i.MatchLdftn(out tar) && tar.DeclaringType is TypeDefinition def && def.Name.StartsWith("<"),
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
                    ic.RemoveRange(8);
                    ic.MoveAfterLabels();
                    ic.EmitLdtoken(me!);
                    def.Resolve();
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
