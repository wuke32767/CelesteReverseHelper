using Celeste.Mod.ReverseHelper.Entities;
using Celeste.Mod.ReverseHelper.SourceGen.Loader;
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
                    .GetType("Celeste.Mod.VortexHelper.VortexHelperModule")?
                    .GetField("PurpleBoosterSpriteBank", bf);
                AttachedJumpThru.LoadContent();
            }
            public static class AttachedJumpThru
            {
                static Type? Type;
                public static MethodInfo? MoveHExact;
                public static void LoadContent()
                {
                    Type = VortexHelperAssembly?
                        .GetType("Celeste.Mod.VortexHelper.Entities.AttachedJumpThru");
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
                }


                public static void activateHooks()
                {
                    activatehooks?.Invoke(null, []);
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

                public static FieldInfo? ReverseHelperSupported;
                public static void LoadContent()
                {
                    Type = Assembly?.GetType("Celeste.Mod.IsaGrabBag.DreamSpinnerRenderer");
                    BeforeRender = Type?.GetMethod("BeforeRender", bf);
                    Render = Type?.GetMethod("Render", bf);
                    GetSpinnersToRender = Type?.GetMethod("GetSpinnersToRender", bf);

                    ReverseHelperSupported = Type?.GetField("_ReverseHelperSupported", bf);
                }
            }
        }
        [ModImportName("SpeedrunTool.SaveLoad")]
        public static class SpeedRunTool_Interop
        {
            public static Func<Type, string[], object>? RegisterStaticTypes;
            public static Action<object>? Unregister;
        }
        public class BGswitch
        {
            [ModImportName("BGswitch")]
            public static class Interop
            {
                // Returns the current BG mode
                public static Func<bool> IsBGMode;

                // Sets the BG mode
                //   bool bgMode: the value to set the BG mode to
                //   bool persistent: whether or not the value should be saved to the session
                public static Action<bool, bool> SetBGMode;

                // Creates and returns a BGModeListener as a Component.
                //   Action<bool> action: the delegate that will be called when the BG mode changes
                public static Func<Action<bool>, Component> GetBGModeListener;
            }
        }
        public static void Load()
        {
            // later it will be moved
            Interop.RegisterDreamBlockLike(CommunalHelper.DreamTunnelEntry.Type,
                (e) => CommunalHelper.DreamTunnelEntry.ActivateNoRoutine?.Invoke(e, null),
                (e) => CommunalHelper.DreamTunnelEntry.DeactivateNoRoutine?.Invoke(e, null)
                );
        }
        [LoadContent]
        public static void LoadContent()
        {
            VortexHelperModule.LoadContent();
            MaddieHelpingHandModule.LoadContent();
            IsaGrabBag.LoadContent();
            CommunalHelper.LoadContent();

            CustomInvisibleBarrier.LoadContent();

        }


        [SourceGen.Loader.Unload]
        public static void Unload()
        {

            SidewaysJumpThru.SomeLambdaHook?.Dispose();
            SidewaysJumpThru.SomeLambdaHook = null;
            SidewaysJumpThru.checkCollisionWithSidewaysJumpthruWhileMovingHook?.Dispose();
            SidewaysJumpThru.checkCollisionWithSidewaysJumpthruWhileMovingHook = null;
        }

    }
}