using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Reflection;

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
            if ((Engine.Scene is Level level) && level.Session.Inventory.DreamDash != isactivated)
            {
                ForceUpdate();
            }
        }
        private static FieldInfo playerHasDreamDashInfo = typeof(DreamBlock).GetField("playerHasDreamDash", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

        public static void ForceUpdate()
        {
            if (Engine.Scene is Level level)
            {
                isactivated = level.Session.Inventory.DreamDash;
                foreach (var v in level.Tracker.GetComponents<DreamToggleListener>())
                {
                    (v as DreamToggleListener)!.OnToggle(isactivated);
                }
                foreach (var v in level.Tracker.GetEntities<DreamBlock>())
                {
                    var vv = (v as DreamBlock)!;
                    if (ReversedDreamBlock.dreamblock_enabled(vv))
                    {
                        playerHasDreamDashInfo.SetValue(vv, false);//for brokemia
                        vv.ActivateNoRoutine();
                    }
                    else
                    {
                        playerHasDreamDashInfo.SetValue(vv, true);//for brokemia
                        vv.DeactivateNoRoutine();
                    }
                }
                foreach (var (t, call) in ReversedDreamBlock.ExternalDreamBlockLike)
                {
                    if (level.Tracker.Entities.TryGetValue(t, out var list))
                    {
                        foreach (var v in list)
                        {
                            if (ReversedDreamBlock.dreamblock_enabled(v))
                            {
                                call.activate?.Invoke(v);
                            }
                            else
                            {
                                call.deactivate?.Invoke(v);
                            }
                        }

                    }
                }
            }
        }

        public static void Load()
        {
            On.Celeste.Level.Update += Level_Update;
            sr = ReverseHelperExtern.SpeedRunTool_Interop.RegisterStaticTypes?.Invoke(typeof(DreamToggleListener), [nameof(isactivated)]);
        }
        static object? sr;
        private static void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            ImmediateUpdate();
        }
        public static void Unload()
        {
            On.Celeste.Level.Update -= Level_Update;
            ReverseHelperExtern.SpeedRunTool_Interop.Unregister?.Invoke(sr!);
        }

        public Action<bool> OnToggle;
    }
}