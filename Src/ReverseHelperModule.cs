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
    internal partial class ReverseHelperModule : EverestModule
    {
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
                Logger.Log(LogLevel.Warn, nameof(ReverseHelper), "Failed when getting DreamDash. Current scene type: " + Engine.Scene.GetType().FullName + "\n" + trace.ToString());
            }
            tar.val = true;
            return ref qwertyuiop;
        }

        public static bool playerHasDreamDash
        {
            get => (Engine.Scene as Level)?.Session.Inventory.DreamDash
                ?? (Engine.Scene as LevelLoader)?.Level.Session.Inventory.DreamDash
                ?? false;
            set => (Engine.Scene as Level)!.Session.Inventory.DreamDash = value;
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