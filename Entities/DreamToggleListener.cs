using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [Tracked(false)]
    public class DreamToggleListener : Component
    {

        public DreamToggleListener(Action<bool> cb) : base(true, false)
        {
            OnToggle = cb;
        }
        public static bool isactivated;
        public static void ImmediateUpdate()
        {
            if((Engine.Scene is Level level)&&level.Session.Inventory.DreamDash != isactivated)
            {
                ForceUpdate();
            }
        }
        public static void ForceUpdate()
        {
            if (Engine.Scene is Level level)
            {
                isactivated = level.Session.Inventory.DreamDash;
                foreach(var v in level.Tracker.GetComponents<DreamToggleListener>())
                {
                    (v as DreamToggleListener)!.OnToggle(isactivated);
                }
                foreach(var v in level.Tracker.GetEntities<DreamBlock>())
                {
                    var vv = (v as DreamBlock)!;
                    if(ReversedDreamBlock.dreamblock_enabled(vv))
                    {
                        vv.ActivateNoRoutine();
                    }
                    else
                    {
                        vv.DeactivateNoRoutine();
                    }
                }
            }
        }

        public static void Load()
        {
            On.Celeste.Level.Update += Level_Update;
        }

        private static void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            ImmediateUpdate();
        }
        public static void Unload()
        {
            On.Celeste.Level.Update -= Level_Update;
        }

        public Action<bool> OnToggle;
    }
}