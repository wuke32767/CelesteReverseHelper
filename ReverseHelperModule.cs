using Celeste.Mod.ReverseHelper.Entities;
using Monocle;
using System;
using System.Runtime.Serialization;

namespace Celeste.Mod.ReverseHelper
{
    public class ReverseHelperModule : EverestModule
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
            Instance = this;
        }

        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);
            ReverseHelperExtern.LoadContent();

            AnotherPurpleBooster.Hooks.LoadContent();
        }

        public override void Load()
        {
            AnotherPurpleBooster.Hooks.Hook();

            HoldableRefill.Load();
            LongDashRefill.Load();
            ForceyHoldables.Load();
            CBAreaComponent.Load();
            DreamToggleListener.Load();
            ReversedDreamBlock.Load();
            CustomInvisibleBarrier.Load();

            //Everest.Events.Level.OnLoadLevel += Level_OnLoadLevel;
        }
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

        public override void Unload()
        {
            AnotherPurpleBooster.Hooks.Unhook();

            HoldableRefill.Unload();
            LongDashRefill.Unload();
            ForceyHoldables.Unload();
            CBAreaComponent.Unload();
            DreamToggleListener.Unload();
            ReversedDreamBlock.Unload();
            CustomInvisibleBarrier.Unload();

            ReverseHelperExtern.Unload();
            //Everest.Events.Level.OnLoadLevel -= Level_OnLoadLevel;
        }
        //public override void PrepareMapDataProcessors(MapDataFixup context)
        //{
        //    base.PrepareMapDataProcessors(context);

        //    context.Add<ReverseHelperMapDataProcessor>();
        //}
        public static bool failed_to_hook_reverse_dreamblock = false;
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