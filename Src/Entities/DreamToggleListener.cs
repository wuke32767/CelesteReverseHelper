﻿namespace Celeste.Mod.ReverseHelper.Entities
{
    using Libraries;
    [Tracked(false)]
    public class DreamToggleListener : Component
    {
        static DreamToggleListener()
        {
            OnScene<DreamToggleListener>.OnUpdate = ImmediateUpdate;
        }
        public DreamToggleListener(Action<bool> cb) : base(true, false)
        {
            OnToggle = cb;
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            initialize();
        }
        void initialize()
        {
            if (Scene is Level level && OnScene<DreamToggleListener>.Construct(Scene))
            {
                isactivated = level.Session.Inventory.DreamDash;
                OnScene<DreamToggleListener>.self!.Depth = 100_000_000;
            }
        }
        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            initialize();
        }
        static bool isactivated;
        public static void ImmediateUpdate(Scene scene)
        {
            scene ??= Engine.Scene;
            if ((scene is Level level) && level.Session.Inventory.DreamDash != isactivated)
            {
                ForceUpdate(level);
            }
        }

        public static void ForceUpdate(Level? level)
        {
            level ??= Engine.Scene as Level;
            if (level is not null)
            {
                isactivated = level.Session.Inventory.DreamDash;
                foreach (var v in level.Tracker.GetComponents<DreamToggleListener>())
                {
                    (v as DreamToggleListener)!.OnToggle(isactivated);
                }
                foreach (var v in level.Tracker.GetEntities<DreamBlock>())
                {
                    var vv = (v as DreamBlock)!;
                    if (DreamBlockConfigurer.dreamblock_enabled(vv))
                    {
                        vv.playerHasDreamDash = false;//for brokemia
                        vv.ActivateNoRoutine();
                    }
                    else
                    {
                        vv.playerHasDreamDash = true;//for brokemia
                        vv.DeactivateNoRoutine();
                    }
                }
                foreach (var (t, call) in DreamBlockConfigurer.ExternalDreamBlockLike)
                {
                    if (level.Tracker.Entities.TryGetValue(t, out var list))
                    {
                        foreach (var v in list)
                        {
                            if (DreamBlockConfigurer.dreamblock_enabled(v))
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
        [SourceGen.Loader.Load]
        public static void Load()
        {
            //On.Celeste.Level.Update += Level_Update;
            sr = ReverseHelperExtern.SpeedRunTool_Interop.RegisterStaticTypes?.Invoke(typeof(DreamToggleListener), [nameof(isactivated)]);
        }
        static object? sr;
        private static void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            ImmediateUpdate(self);
        }
        [SourceGen.Loader.Unload]
        public static void Unload()
        {
            //On.Celeste.Level.Update -= Level_Update;
            ReverseHelperExtern.SpeedRunTool_Interop.Unregister?.Invoke(sr!);
        }

        public Action<bool> OnToggle;
    }
}