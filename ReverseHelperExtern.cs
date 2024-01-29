using Celeste.Mod.ReverseHelper.Entities;
using FMOD;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using static Celeste.Mod.ReverseHelper.ReverseHelperExtern.MaddieHelpingHandModule;

namespace Celeste.Mod.ReverseHelper
{
    public static class ReverseHelperExtern
    {
        const BindingFlags bf = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        public static class VortexHelperModule
        {
            static FieldInfo? purpleBoosterSpriteBank;
            public static SpriteBank? PurpleBoosterSpriteBank { get => (SpriteBank?)purpleBoosterSpriteBank?.GetValue(null); }
            static Assembly? VortexHelperAssembly;
            public static void LoadContent()
            {
                VortexHelperAssembly =
                    AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(assembly => assembly.GetName().Name == "VortexHelper")
                    .FirstOrDefault();
                purpleBoosterSpriteBank =
                    VortexHelperAssembly?
                    .GetType("Celeste.Mod.VortexHelper.VortexHelperModule")
                    .GetField("PurpleBoosterSpriteBank", bf);
                AttachedJumpThru.LoadContent();
            }
            public static class AttachedJumpThru
            {
                static Type? Type;
                static ConstructorInfo? Ctor;
                public static MethodInfo? MoveHExact;
                public static JumpThru? ctor(EntityData data, Vector2 offset)
                => Ctor?.Invoke([data, offset]) as JumpThru;
                public static void LoadContent()
                {
                    Type = VortexHelperAssembly?
                        .GetType("Celeste.Mod.VortexHelper.Entities.AttachedJumpThru");
                    Ctor = Type?.GetConstructor([typeof(EntityData), typeof(Vector2)]);
                    MoveHExact = Type?.GetMethod("MoveHExact", bf);
                }
            }
        }
        public static class MaddieHelpingHandModule
        {
            static Assembly? Assembly;
            public static void LoadContent()
            {
                Assembly =
                    AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(assembly => assembly.GetName().Name == "MaxHelpingHand")
                    .FirstOrDefault();
                AttachedSidewaysJumpThru.LoadContent();
                SidewaysJumpThru.LoadContent();
            }
            public static class AttachedSidewaysJumpThru
            {
                static Type? Type;

                //so easy, but it can't process return value.
                static FieldInfo? OnDashCollide;

                static ConstructorInfo? Ctor;
                public static void LoadContent()
                {
                    Type = Assembly?.GetType("Celeste.Mod.MaxHelpingHand.Entities.AttachedSidewaysJumpThru");
                    Ctor = Type?.GetConstructor([typeof(EntityData), typeof(Vector2)]);
                    OnDashCollide = Type?.GetField("OnDashCollide", bf);
                }
                public static Entity? ctor(EntityData data, Vector2 offset)
                => Ctor?.Invoke([data, offset]) as Entity;
                public static void SetIfNull_OnDashCollide(Entity e, DashCollision dc)
                {
                    if (OnDashCollide?.GetValue(e) is null)
                    {
                        OnDashCollide?.SetValue(e, dc);
                    }
                }
            }
            public static class SidewaysJumpThru
            {
                static Type? Type;

                // static FieldInfo? OnDashCollide;
                static MethodInfo? addSidewaysJumpthrusInHorizontalMoveMethods;
                static MethodInfo? checkCollisionWithSidewaysJumpthruWhileMoving;
                static MethodInfo? activatehooks;
                public static ILHook? SomeLambdaHook;
                public static ILHook? checkCollisionWithSidewaysJumpthruWhileMovingHook;
                public static MethodInfo? TheTarget;

                public static DashCollision? dashCollision;
                public static bool failed = false;
                public static void LoadContent()
                {
                    Type = Assembly?.GetType("Celeste.Mod.MaxHelpingHand.Entities.SidewaysJumpThru");
                    addSidewaysJumpthrusInHorizontalMoveMethods = Type?.GetMethod("addSidewaysJumpthrusInHorizontalMoveMethods", bf);
                    checkCollisionWithSidewaysJumpthruWhileMoving = Type?.GetMethod("checkCollisionWithSidewaysJumpthruWhileMoving", bf);
                    if (TheTarget is null)
                    {
                        new ILHook(addSidewaysJumpthrusInHorizontalMoveMethods, Watcher).Dispose();
                    }
                    if (TheTarget is not null)
                    {
                        checkCollisionWithSidewaysJumpthruWhileMovingHook = new ILHook(checkCollisionWithSidewaysJumpthruWhileMoving, Pilferer);
                        SomeLambdaHook = new ILHook(TheTarget, Modifier);
                    }
                    activatehooks = Type?.GetMethod("activateHooks", bf);
                    if (!failed)
                    {
                        Logger.Log(LogLevel.Warn, "ReverseHelper", "Nothing went wrong. This warning is just to tell you I hooked MaddieHelpingHand's SidewaysJumpThru, and I'm not sure if it is safe. If anything went wrong, please ping USSRNAME.");
                        Logger.Log(LogLevel.Warn, "ReverseHelper", "Only SidewaysJumpThru could went wrong, I think. (and should not crash. (if maddie havn't changed SidewaysJumpThru.))");
                    }
                }
                public static void activateHooks()
                {
                    activatehooks?.Invoke(null, []);
                }
                public static void Watcher(ILContext il)
                {
                    MethodReference? inst3;
                    Type inst4;
                    ILCursor ilc = new(il);
                    if (ilc.TryGotoNext(i => { i.MatchCallvirt(out var m); return m?.Name == "EmitDelegate"; }) && ilc.TryGotoPrev(i => { return i.MatchLdftn(out _); }))
                    {
                        inst3 = ilc.Next.Operand as MethodReference;
                        inst4 = Assembly!.GetType(inst3!.DeclaringType.FullName.Replace('/', '+'));
                        TheTarget = inst4.GetMethod(inst3.Name, bf);
                        /*
                        39	007A	ldloc.0
                        40	007B	ldsfld	class [mscorlib]System.Func`4<class [Celeste]Celeste.Solid, class [Celeste]Monocle.Entity, int32, class [Celeste]Celeste.Solid> Celeste.Mod.MaxHelpingHand.Entities.SidewaysJumpThru/'<>c'::'<>9__11_1'
                        41	0080	dup
                        42	0081	brtrue.s	49 (009A) callvirt instance int32 [MonoMod.Utils]MonoMod.Cil.ILCursor::EmitDelegate<class [mscorlib]System.Func`4<class [Celeste]Celeste.Solid, class [Celeste]Monocle.Entity, int32, class [Celeste]Celeste.Solid>>(!!0)
                        43	0083	pop
                        44	0084	ldsfld	class Celeste.Mod.MaxHelpingHand.Entities.SidewaysJumpThru/'<>c' Celeste.Mod.MaxHelpingHand.Entities.SidewaysJumpThru/'<>c'::'<>9'
                        45	0089	ldftn	instance class [Celeste]Celeste.Solid Celeste.Mod.MaxHelpingHand.Entities.SidewaysJumpThru/'<>c'::'<addSidewaysJumpthrusInHorizontalMoveMethods>b__11_1'(class [Celeste]Celeste.Solid, class [Celeste]Monocle.Entity, int32)
                        46	008F	newobj	instance void class [mscorlib]System.Func`4<class [Celeste]Celeste.Solid, class [Celeste]Monocle.Entity, int32, class [Celeste]Celeste.Solid>::.ctor(object, native int)
                        47	0094	dup
                        48	0095	stsfld	class [mscorlib]System.Func`4<class [Celeste]Celeste.Solid, class [Celeste]Monocle.Entity, int32, class [Celeste]Celeste.Solid> Celeste.Mod.MaxHelpingHand.Entities.SidewaysJumpThru/'<>c'::'<>9__11_1'
                        49	009A	callvirt	instance int32 [MonoMod.Utils]MonoMod.Cil.ILCursor::EmitDelegate<class [mscorlib]System.Func`4<class [Celeste]Celeste.Solid, class [Celeste]Monocle.Entity, int32, class [Celeste]Celeste.Solid>>(!!0)
                        50	009F	pop
                         */
                    }
                    else
                    {
                        Logger.Log(LogLevel.Error, "ReverseHelper", $"Failed when hooking MaddieHelpingHand. Here's The {nameof(Watcher)}.");
                    }
                }
                public static void Modifier(ILContext il)
                {
                    ILCursor ilc = new(il);
                    if (ilc.TryGotoNext(MoveType.After, i => i.MatchNewobj(out _)))
                    {
                        ilc.EmitDelegate((Entity e) =>
                        {
                            (e as Solid)!.OnDashCollide ??= dashCollision;
                            return e;
                        });
                    }
                    else
                    {
                        Logger.Log(LogLevel.Error, "ReverseHelper", $"Failed when hooking MaddieHelpingHand. Here's The {nameof(Modifier)}.");
                    }
                }
                public static void Pilferer(ILContext il)
                {
                    ILCursor ilc = new(il);
                    if (ilc.TryGotoNext(i => { i.MatchCallvirt(out var m); return m?.Name == "Invoke"; }))
                    {
                        ilc.Remove();
                        ilc.EmitDelegate((DashCollision self, Player p, Vector2 v) =>
                        {
                            dashCollision = self;
                            return DashCollisionResults.NormalCollision;
                        });
                    }
                    else
                    {
                        Logger.Log(LogLevel.Error, "ReverseHelper", $"Failed when hooking MaddieHelpingHand. Here's The {nameof(Pilferer)}.");
                    }
                }
            }
        }

        public static class ExtendedVariantsModule
        {
            private static object? triggermanager;
            private static Assembly? ExtendedVariantMode;
            private static Type? module;
            private static object? instance;
            private static Type? manager;
            private static MethodInfo? managerExit;
            private static MethodInfo? managerEnter;
            private static Type? Variant;
            public struct as_triggermamager
            {
                internal void LongDash_OnExitedRevertOnLeaveTrigger(float dashTime, bool legacy)
                {
                    managerExit?.Invoke(triggermanager, bf, null, [/*Convert.ChangeType(9, Variant)*/9, dashTime, legacy], null);
                }

                internal void LongDash_OnEnteredInTrigger(float dashTime, bool revertOnLeave, bool isFade, bool revertOnDeath, bool legacy)
                {
                    managerEnter?.Invoke(triggermanager, bf, null, [/*Convert.ChangeType(9, Variant)*/9, dashTime, revertOnLeave, isFade, revertOnDeath, legacy], null);

                }
            }

            public static as_triggermamager TriggerManager;

            public static void LoadContent()
            {
                ExtendedVariantMode = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(assembly => assembly.GetName().Name == "ExtendedVariantMode")
                    .FirstOrDefault();
                if (ExtendedVariantMode is null)
                {
                    return;
                }
                module = ExtendedVariantMode.GetType("ExtendedVariants.Module.ExtendedVariantsModule");
                instance = module.GetField("Instance", bf).GetValue(null);
                triggermanager = module.GetField("TriggerManager", bf).GetValue(instance);
                manager = ExtendedVariantMode.GetType("ExtendedVariants.ExtendedVariantTriggerManager");
                managerExit = manager.GetMethod("OnExitedRevertOnLeaveTrigger");
                managerEnter = manager.GetMethod("OnEnteredInTrigger");
                Variant = ExtendedVariantMode.GetType("ExtendedVariants.Module.ExtendedVariantsModule.Variant");
            }
        }
        public static class GravityHelperModule
        {
            static Assembly? Assembly;
            static Type? Module;
            static Type? HookLevel;
            static MethodInfo? updatehooks;
            static FieldInfo? CurrentHookLevel;
            static int? Everything;
            //public static string[] all;
            [MethodImpl(MethodImplOptions.NoOptimization)]
            public static void LoadContent()
            {
                Assembly =
                    AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(assembly => assembly.GetName().Name == "GravityHelper")
                    .FirstOrDefault();
                Module = Assembly?.GetType("Celeste.Mod.GravityHelper.GravityHelperModule");
                HookLevel = Assembly?.GetType("Celeste.Mod.GravityHelper.GravityHelperModule+HookLevel");
                //all = Assembly?.GetTypes().Select(x=>x.FullName).ToArray();
                updatehooks = Module?.GetMethod("updateHooks", bf);
                CurrentHookLevel = Module?.GetField("CurrentHookLevel", bf);

                var t = HookLevel?.GetEnumValues().Cast<object>().FirstOrDefault(x => x.ToString() == "Everything");
                if(t is not null)
                {
                    Everything = (int)t;
                }
                UpsideDownJumpThru.LoadContent();
            }
            public static void RequireGravityHelperHook()
            {
                if(Everything is not null&&CurrentHookLevel is not null&&(int)CurrentHookLevel.GetValue(null)!= Everything)
                {
                    updatehooks?.Invoke(null, [Everything]);
                }
            }
            public static class UpsideDownJumpThru
            {
                static Type? Type;

                static ConstructorInfo? Ctor;
                public static JumpThru? ctor(EntityData data, Vector2 offset)
                    => Ctor?.Invoke([data, offset]) as JumpThru;

                public static void LoadContent()
                {
                    Type = Assembly?.GetType("Celeste.Mod.GravityHelper.Entities.UpsideDownJumpThru");
                    Ctor = Type?.GetConstructor([typeof(EntityData), typeof(Vector2)]);

                }
            }
        }
        public static class IsaGrabBag
        {
            static Assembly? Assembly;
            public static void LoadContent()
            {
                Assembly =
                    AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(assembly => assembly.GetName().Name == "IsaMods")
                    .FirstOrDefault();
                DreamSpinner.LoadContent();
                DreamSpinnerRenderer.LoadContent();
            }
            public static class DreamSpinner
            {
                static Type? Type;

                public static MethodInfo? Update;
                static FieldInfo? color;
                static FieldInfo? OneUse;
                static FieldInfo? block;

                public static void LoadContent()
                {
                    Type = Assembly?.GetType("Celeste.Mod.IsaGrabBag.DreamSpinner");
                    Update = Type?.GetMethod("Update",bf);
                    color = Type?.GetField("color",bf);
                    OneUse = Type?.GetField("OneUse", bf);
                    block = Type?.GetField("block", bf);
                }
                public static void set_color(Entity e,Color c)
                {
                    color!.SetValue(e, c);
                }
                public static bool get_OneUse(Entity e)
                {
                    return (bool)OneUse!.GetValue(e);
                }
                public static Entity get_block(Entity e)
                {
                    return (Entity)block!.GetValue(e);
                }
            }
            public static class DreamSpinnerRenderer
            {
                static Type? Type;

                // static FieldInfo? OnDashCollide;
                static MethodInfo? BeforeHook;
                public static ILHook? SomeLambdaHook;

                public static void LoadContent()
                {
                    Type = Assembly?.GetType("Celeste.Mod.MaxHelpingHand.Entities.SidewaysJumpThru");
                    BeforeHook = Type?.GetMethod("BeforeRender", bf);
                }
            }
        }
        public static string[] debugger;//what if ReverseHelper named as ConverseHelper in assembly?
        public static Assembly[] debugger_;
        public static void LoadContent()
        {
            VortexHelperModule.LoadContent();
            ExtendedVariantsModule.LoadContent();
            MaddieHelpingHandModule.LoadContent();
            GravityHelperModule.LoadContent();
            IsaGrabBag.LoadContent();

            CustomInvisibleBarrier.LoadContent();
            ReversedDreamBlock.LoadContent();
            if(false)
            {
                debugger_ = AppDomain.CurrentDomain
                    .GetAssemblies();
                debugger=debugger_.Select(x=>x.GetName().Name).ToArray();
            }
        }


        public static void Unload()
        {

            SidewaysJumpThru.SomeLambdaHook?.Dispose();
            SidewaysJumpThru.SomeLambdaHook = null;
            SidewaysJumpThru.checkCollisionWithSidewaysJumpthruWhileMovingHook?.Dispose();
            SidewaysJumpThru.checkCollisionWithSidewaysJumpthruWhileMovingHook = null;
        }

    }
}