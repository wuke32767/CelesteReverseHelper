using Celeste.Mod.ReverseHelper.Entities;
using Celeste.Mod.ReverseHelper.Libraries;
using Celeste.Mod.ReverseHelper.SourceGen.Loader;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.ModInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using static Celeste.Mod.ReverseHelper.ReverseHelperExtern;

namespace Celeste.Mod.ReverseHelper
{
    interface SingleSession<T>
    {
        public void Write(T target);
        public void Copy(T from);
    }
    class ReverseHelperSession : EverestModuleSession
    {
        public Triggers.EnableTrigger.Session EnableTrigger { get; set; } = new();
    }
    class ReverseHelperSessionHelper
    {
        public static ReverseHelperSession Fake => ReverseHelperModule.Session;
        public static ReverseHelperSession Real => ReverseHelperModule.RealSession;
        public static void Write()
        {
            foreach (var (k, v) in Fake.EnableTrigger.Triggered)
            {
                Real.EnableTrigger.Triggered[k] = v;
            }
            Fake.EnableTrigger.Triggered.Clear();
        }
        public static void Revert()
        {
            Fake.EnableTrigger.Triggered.Clear();
        }
        [Load]
        public static void Load()
        {
            Everest.Events.Level.OnTransitionTo += Level_OnTransitionTo;
            Everest.Events.Level.OnLoadLevel += Level_OnLoadLevel;
        }

        private static void Level_OnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            Revert();
        }

        private static void Level_OnTransitionTo(Level level, LevelData next, Vector2 direction)
        {
            Write();
        }

        [Unload]
        public static void Unload()
        {
            Everest.Events.Level.OnTransitionTo -= Level_OnTransitionTo;
            Everest.Events.Level.OnTransitionTo -= Level_OnTransitionTo;
        }
    }
    internal partial class ReverseHelperModule : EverestModule
    {
        static ReverseHelperSession FakeSession = new();
        public override Type SessionType => typeof(ReverseHelperSession);
        public static ReverseHelperSession Session => FakeSession;
        public static ReverseHelperSession RealSession => (ReverseHelperSession)Instance._Session;
        static bool qwertyuiop;
        static ConditionalWeakTable<Scene, Box<bool>> tested = [];
        public static ref bool playerHasDreamDashBetter(Entity entity)
        {
            if (entity?.Scene is Level level)
            {
                return ref level.Session.Inventory.DreamDash;
            }
            var session = Engine.Scene switch
            {
                Level level2 => level2.Session,
                LevelLoader { Level: Level level3 } => level3.Session,
                AssetReloadHelper { OrigScene: Level level4 } => level4.Session,
                LevelExit { session: Session session2 } => session2,
                _ => null,
            };
            if (session is not null)
            {
                return ref session.Inventory.DreamDash;
            }

            var tar = tested.GetOrCreateValue(Engine.Scene);
            if (!tar.val)
            {
                StackTrace trace = new StackTrace();
                Logger.Log(LogLevel.Error, nameof(ReverseHelper), "Failed when getting DreamDash. Current scene type: " + Engine.Scene.GetType().FullName + " " + Engine.Scene.GetType().Module.ScopeName + "\n" + trace.ToString());
            }
            tar.val = true;
            qwertyuiop = false;
            return ref qwertyuiop;
        }

        public static bool playerHasDreamDash
        {
            get => playerHasDreamDashBetter(null!);
            set => playerHasDreamDashBetter(null!) = value;
        }

#pragma warning disable CS8618
        public static ReverseHelperModule Instance;
#pragma warning restore CS8618
        public static int AnotherPurpleBoosterDashState;
        public static int AnotherPurpleBoosterState;

        public ReverseHelperModule()
        {
            ReverseHelperExtern.Load();
            Instance = this;
        }
        public override void LoadContent(bool firstLoad)
        {
            LoadContent();
        }
        [LoadContenter]
        public new partial void LoadContent();

        [Loader]
        public override partial void Load();
        public static bool ReversedDreamBlockContainerHooked = false;
        //private void Level_OnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        //{
        //    if (ReverseHelperMapDataProcessor.DreamEnable.Contains(level.Session.MapData.Data.GetSID()))
        //    {
        //        if (!ReversedDreamBlockContainerHooked)
        //        {
        //            ReversedDreamBlock.Load();
        //            ReversedDreamBlockContainerHooked = true;
        //        }
        //    }
        //    else
        //    {
        //        if (ReversedDreamBlockContainerHooked)
        //        {
        //            ReversedDreamBlock.Unload();
        //            ReversedDreamBlockContainerHooked = false;
        //        }

        //    }
        //}

        [Unloader]
        public override partial void Unload();
        //public override void PrepareMapDataProcessors(MapDataFixup context)
        //{
        //    base.PrepareMapDataProcessors(context);
        //    context.Add<ReverseHelperMapDataProcessor>();
        //}
        public static bool failed_to_hook_reverse_dreamblock = false;
        [Load]
        public static void PrepareLazyLoad()
        {
            On.Celeste.LevelLoader.ctor += onLevelLoad;
            On.Celeste.OverworldLoader.ctor += onOverworldLoad;
        }
        private static void onOverworldLoad(On.Celeste.OverworldLoader.orig_ctor orig, OverworldLoader self, Overworld.StartMode startMode, HiresSnow snow)
        {
            orig(self, startMode, snow);
            Clear();
        }
        private static void Clear()
        {
            foreach (var (_, unl) in lazylist)
            {
                unl.unload();
            }
        }
        private static void onLevelLoad(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition)
        {
            orig(self, session, startPosition);
            if (lazylist.Count > 0)
            {
                foreach (var name in session.MapData?.Levels?.SelectMany(level => level.Entities?.Select(entity => entity.Name) ?? []) ?? [])
                {
                    if (lazylist.TryGetValue(name, out var val))
                    {
                        val.load();
                    }
                }
            }
        }
        [LazyLoadDirectory]
        static Dictionary<string, (Action load, Action unload)> lazylist = new();
        [Unload]
        public static void UnloadLazyLoad()
        {
            On.Celeste.LevelLoader.ctor -= onLevelLoad;
            On.Celeste.OverworldLoader.ctor -= onOverworldLoad;
            Clear();
        }
    }
    class NotTestedAttribute : Attribute { }
    class WIPAttribute(string msg="") : Attribute { }
}

public class ReverseHelperILHookException : Exception
{
    public ReverseHelperILHookException()
    {
    }

    public ReverseHelperILHookException(string message) : base(message)
    {
    }

    public ReverseHelperILHookException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ReverseHelperILHookException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}