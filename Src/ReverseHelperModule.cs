using Celeste.Mod.ReverseHelper.Entities;
using Celeste.Mod.ReverseHelper.SourceGen.Loader;
using Monocle;
using MonoMod.ModInterop;
using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using static Celeste.Mod.ReverseHelper.ReverseHelperExtern;

namespace Celeste.Mod.ReverseHelper
{
    internal partial class ReverseHelperModule : EverestModule
    {
        public static bool playerHasDreamDash
        {
            get => (Engine.Scene as Level)?.Session.Inventory.DreamDash ?? (Engine.Scene as LevelLoader).Level.Session.Inventory.DreamDash;
            set => (Engine.Scene as Level).Session.Inventory.DreamDash = value;
        }

        public static ReverseHelperModule Instance;
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
    }
    namespace SourceGen.Loader
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class LoadAttribute : Attribute
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