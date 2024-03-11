using Celeste.Mod.ReverseHelper.Entities;
using FMOD;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.ModInterop;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
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
                    activatehooks = Type?.GetMethod("activateHooks", bf);
                    Load();
                }

                private static void Load()
                {
                    if (TheTarget is null)
                    {
                        new ILHook(addSidewaysJumpthrusInHorizontalMoveMethods, Watcher).Dispose();
                    }
                    if (TheTarget is not null)
                    {
                        checkCollisionWithSidewaysJumpthruWhileMovingHook = new ILHook(checkCollisionWithSidewaysJumpthruWhileMoving, Pilferer);
                        SomeLambdaHook = new ILHook(TheTarget, Modifier);
                    }
                    if (!failed)
                    {
                        Logger.Log(LogLevel.Warn, "ReverseHelper", "Nothing went wrong. This warning is just to tell you I hooked MaddieHelpingHand(MaxHelpingHand)'s SidewaysJumpThru, and I'm not sure if it is safe. If anything went wrong, please ping USSRNAME.");
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
                        Logger.Log(LogLevel.Error, "ReverseHelper", $"Failed when hooking MaddieHelpingHand. Here's The Part1.");
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
                            dashCollision = null;
                            return e;
                        });
                    }
                    else
                    {
                        Logger.Log(LogLevel.Error, "ReverseHelper", $"Failed when hooking MaddieHelpingHand. Here's The Part3.");
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
                        Logger.Log(LogLevel.Error, "ReverseHelper", $"Failed when hooking MaddieHelpingHand. Here's The Part2.");
                    }
                }
            }
        }

        public static class ExtendedVariantsModule
        {
            [ModImportName("ExtendedVariantMode")]
            public static class Interop
            {
                public static Func<string, object> GetCurrentVariantValue;

                public static Action<string, int, bool> TriggerIntegerVariant;

                public static Action<string, bool, bool> TriggerBooleanVariant;

                public static Action<string, float, bool> TriggerFloatVariant;

                public static Action<string, object, bool> TriggerVariant;

                // Token: 0x06000793 RID: 1939 RVA: 0x0001A5A1 File Offset: 0x000187A1
                public static Action<int> SetJumpCount;

                // Token: 0x06000794 RID: 1940 RVA: 0x0001A5AA File Offset: 0x000187AA
                public static Action<int> CapJumpCount;

            }
            public static void LoadContent()
            {
                typeof(Interop).ModInterop();
            }
        }
        public static class CommunalHelper
        {
            static Assembly? Assembly;
            public static void LoadContent()
            {
                Assembly =
                    AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(assembly => assembly.GetName().Name == "CommunalHelper")
                    .FirstOrDefault();
                DreamTunnelEntry.LoadContent();
            }
            public static class DreamTunnelEntry
            {
                public static Type? Type;
                public static MethodInfo? Added;
                public static MethodInfo? ActivateNoRoutine;
                public static MethodInfo? DeactivateNoRoutine;
                public static void LoadContent()
                {
                    Type = Assembly?.GetType("Celeste.Mod.CommunalHelper.Entities.DreamTunnelEntry");
                    Added = Type?.GetMethod("Added");
                    ActivateNoRoutine = Type?.GetMethod("ActivateNoRoutine");
                    DeactivateNoRoutine = Type?.GetMethod("DeactivateNoRoutine");
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
                GrabBagModule.LoadContent();
                DreamSpinner.LoadContent();
                DreamSpinnerRenderer.LoadContent();
            }
            public static class GrabBagModule
            {
                public static Type? Type;

                public static PropertyInfo? spritesinfo;

                internal static void LoadContent()
                {
                    Type = Assembly?.GetType("Celeste.Mod.IsaGrabBag.GrabBagModule");
                    spritesinfo = Type?.GetProperty("sprites", bf);
                }
                public static SpriteBank? sprites { get => (SpriteBank)spritesinfo?.GetValue(null); }
            }
            public static class DreamSpinner
            {
                public static Type? Type;

                public static MethodInfo? Update;
                static FieldInfo? color;
                static FieldInfo? OneUse;
                static FieldInfo? block;
                static MethodInfo? InViewr;

                public static void LoadContent()
                {
                    Type = Assembly?.GetType("Celeste.Mod.IsaGrabBag.DreamSpinner");
                    Update = Type?.GetMethod("Update", bf);
                    color = Type?.GetField("color", bf);
                    OneUse = Type?.GetField("OneUse", bf);
                    block = Type?.GetField("block", bf);
                    InViewr = Type?.GetMethod("InView", bf);
                }
                public static void set_color(Entity e, Color c)
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
                public static bool InView(Entity e)
                {
                    return (bool)InViewr!.Invoke(e, []);
                }
            }
            public static class DreamSpinnerRenderer
            {
                static Type? Type;

                // static FieldInfo? OnDashCollide;
                public static MethodInfo? BeforeRender;
                public static MethodInfo? Render;
                public static MethodInfo? GetSpinnersToRender;

                public static void LoadContent()
                {
                    Type = Assembly?.GetType("Celeste.Mod.IsaGrabBag.DreamSpinnerRenderer");
                    BeforeRender = Type?.GetMethod("BeforeRender", bf);
                    Render = Type?.GetMethod("Render", bf);
                    GetSpinnersToRender = Type?.GetMethod("GetSpinnersToRender", bf);
                }
            }
        }
        [ModImportName("SpeedrunTool.SaveLoad")]
        public static class SpeedRunTool_Interop
        {
            public static Func<Type, string[], object>? RegisterStaticTypes;
            public static Action<object>? Unregister;
        }
        //public static string[] debugger;//what if ReverseHelper is named as ConverseHelper in assembly?
        //public static Assembly[] debugger_;


        [ModExportName("ReverseHelper.DreamBlock")]
        public static class Interop
        {
            /// <summary>
            /// if a entity acts like DreamBlock, use this.
            /// it should access Inventory.DreamDash only when awaking, and save it to a field.
            /// then, it controls visuals with that field and [De]ActivateNoRoutine. 
            /// and NO game logic relys on Inventory.DreamDash and that field.
            /// </summary>
            public static void RegisterDreamBlockLike(Type targetType, Action<Entity> ActivateNoRoutine, Action<Entity> DeactivateNoRoutine)
            {
                if(targetType is not null&&ActivateNoRoutine is not null&&DeactivateNoRoutine is not null)
                {
                    ReversedDreamBlock.ExternalDreamBlockLike[targetType] = (ActivateNoRoutine, DeactivateNoRoutine);
                }
            }
            /// <summary>
            /// or, use this to check if your entity is enabled.
            /// notice that because of awake order, better to awake at the end of awake frame.
            /// </summary>
            /// <param name="e">the entity to be checked.</param>
            public static bool PlayerHasDreamDash(Entity e)
            {
                return ReversedDreamBlock.dreamblock_enabled(e);
            }
        }
        public static void Load()
        {
            typeof(Interop).ModInterop();
        }
        public static void LoadContent()
        {
            VortexHelperModule.LoadContent();
            ExtendedVariantsModule.LoadContent();
            MaddieHelpingHandModule.LoadContent();
            IsaGrabBag.LoadContent();
            CommunalHelper.LoadContent();

            CustomInvisibleBarrier.LoadContent();
            ReversedDreamBlock.LoadContent();

            // later it will be moved
            Interop.RegisterDreamBlockLike(CommunalHelper.DreamTunnelEntry.Type,
                (e) => CommunalHelper.DreamTunnelEntry.ActivateNoRoutine?.Invoke(e, null),
                (e) => CommunalHelper.DreamTunnelEntry.DeactivateNoRoutine?.Invoke(e, null)
                );
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