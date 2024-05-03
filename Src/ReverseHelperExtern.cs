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
                static MethodInfo? activatehooks;
                public static MethodInfo? TheTarget;

                public static DashCollision? dashCollision;
                public static bool failed = false;
                public static void LoadContent()
                {
                    Type = Assembly?.GetType("Celeste.Mod.MaxHelpingHand.Entities.SidewaysJumpThru");
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
                public static Func<string, object>? GetCurrentVariantValue;

                public static Action<string, int, bool>? TriggerIntegerVariant;

                public static Action<string, bool, bool>? TriggerBooleanVariant;

                public static Action<string, float, bool>? TriggerFloatVariant;

                public static Action<string, object, bool>? TriggerVariant;

                public static Action<int>? SetJumpCount;

                public static Action<int>? CapJumpCount;

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
                public static SpriteBank? sprites { get => (SpriteBank?)spritesinfo?.GetValue(null); }
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
                public static Func<bool>? IsBGMode;

                // Sets the BG mode
                //   bool bgMode: the value to set the BG mode to
                //   bool persistent: whether or not the value should be saved to the session
                public static Action<bool, bool>? SetBGMode;

                // Creates and returns a BGModeListener as a Component.
                //   Action<bool> action: the delegate that will be called when the BG mode changes
                public static Func<Action<bool>, Component>? GetBGModeListener;
            }
        }
        public static void Load()
        {
            // later it will be moved
            //Interop.RegisterDreamBlockLike(CommunalHelper.DreamTunnelEntry.Type,
            //    (e) => CommunalHelper.DreamTunnelEntry.ActivateNoRoutine?.Invoke(e, null),
            //    (e) => CommunalHelper.DreamTunnelEntry.DeactivateNoRoutine?.Invoke(e, null)
            //    );
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

        }

    }
}