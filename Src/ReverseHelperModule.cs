using Celeste.Mod.ReverseHelper.Entities;
using Celeste.Mod.ReverseHelper.SourceGen.Loader;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.ModInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using static Celeste.Mod.ReverseHelper.ReverseHelperExtern;

namespace Celeste.Mod.ReverseHelper
{
    internal partial class ReverseHelperModule : EverestModule
    {
        public static ref bool playerHasDreamDashBetter(Level level)
        {
            if(level is null)
            {
                return ref (Engine.Scene as Level)!.Session.Inventory.DreamDash;
            }
            return ref level.Session.Inventory.DreamDash;
        }

        public static bool playerHasDreamDash
        {
            get => (Engine.Scene as Level)?.Session.Inventory.DreamDash ?? (Engine.Scene as LevelLoader)?.Level.Session.Inventory.DreamDash ?? false;
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
    namespace SourceGen.Loader
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class LoadAttribute : Attribute
        {
        }
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class LazyLoadAttribute : Attribute
        {
        }
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
        public class LazyLoadDirectoryAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class UnloadAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class LoadContentAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class LoaderAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class UnloaderAttribute : Attribute
        {
        }
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class LoadContenterAttribute : Attribute
        {
        }
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        public class DependencyAttribute(Type type) : Attribute()
        {
        }
    }
    namespace SourceGen
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class GeneratedAttribute : Attribute
        {
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