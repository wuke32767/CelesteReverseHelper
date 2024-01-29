using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Component = Monocle.Component;

namespace Celeste.Mod.ReverseHelper.Entities
{
    public class ReversedDreamBlockComponent() : Component(true, false)
    {

    }


    [CustomEntity("ReverseHelper/ReversedDreamBlockContainer")]
    public class ReversedDreamBlock : Entity
    {

        public new Level Scene { get => base.Scene as Level; }
        private bool bindOnRoomStart;
        //private FlagMatch flagBind;
        //private FlagMatch flagEnable;
        //private TypeMatch wraptype;
        //private float pushForce;

        public override void Update()
        {
            base.Update();
        }
        public static MTexture invalid_img;
        public ReversedDreamBlock(Vector2 position, float width, float height) : base(position)
        {
            Collider = new Hitbox(width, height, 0f, 0f);
            //wraptype = typename;
            //pushForce = force;
            //this.flagBind = flagBind;
            //this.flagEnable = flagEnable;
            bindOnRoomStart = bindOnRoomStart = true;
            invalid_img ??= GFX.Game.GetAtlasSubtextures("__fallback")?[0];
            Visible = ReverseHelperModule.failed_to_hook_reverse_dreamblock;
            Depth = -200000;
        }

        // Token: 0x06000EF3 RID: 3827 RVA: 0x0003A0EC File Offset: 0x000382EC
        public ReversedDreamBlock(EntityData e, Vector2 offset) : this(e.Position + offset, e.Width, e.Height)
        {
        }
        static ILHook dashcoroutine;
        public static void Load()
        {
            IL.Celeste.Player.DreamDashCheck += Player_DreamDashCheck;
            //IL.Celeste.Player.DashCoroutine += Player_DashCoroutine;
            var mi = typeof(Player).GetMethod("DashCoroutine", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetStateMachineTarget();
            dashcoroutine = new ILHook(mi, Player_DashCoroutine);


        }
        static ILHook grabbag_workaround;
        public static void LoadContent()
        {
            var upd = ReverseHelperExtern.IsaGrabBag.DreamSpinner.Update;
            if (upd is not null)
            {
                grabbag_workaround=new ILHook(upd, (ILContext il) =>
                {
                    ILCursor ic = new ILCursor(il);
                    ic.Emit(OpCodes.Ldarg_0);
                    ic.EmitDelegate((Entity e) =>
                        {
                            Color color;
                            color = Color.Black;
                            if (!dreamblock_enabled(ReverseHelperExtern.IsaGrabBag.DreamSpinner.get_block(e)))
                            {
                                color = new Color(25, 25, 25);
                            }
                            else
                            {
                                if (ReverseHelperExtern.IsaGrabBag.DreamSpinner.get_OneUse(e))
                                {
                                    color = new Color(30, 22, 10);
                                }
                            }
                            ReverseHelperExtern.IsaGrabBag.DreamSpinner.set_color(e, color);
                        });
                    while (ic.TryGotoNext(MoveType.After,  (i) => { i.MatchCallvirt(out var v); return v?.Name == "get_Inventory"; },
                        (i) => { i.MatchLdfld(out var v);return v?.Name == "DreamDash"; }))
                    {
                        ic.Emit(OpCodes.Ldarg_0);
                        ic.EmitDelegate((bool x,Entity self) => dreamblock_enabled(ReverseHelperExtern.IsaGrabBag.DreamSpinner.get_block(self)));
                    }
                });

            }
        }

        public override void Render()
        {
            invalid_img?.Draw(Position);
            //base.Render();
        }
        private static void Player_DashCoroutine(ILContext il)
        {
            ILCursor ic = new(il);
            if (ic.TryGotoNext(MoveType.After, (i) => { i.MatchCallvirt(out var v); return v?.Name == "get_Inventory"; },
                (i) => { i.MatchLdfld(out var v); return v?.Name == "DreamDash"; }))
            {
                ic.EmitDelegate((bool x) => true);

                if (ic.TryGotoNext(MoveType.Before, (i) => { i.MatchCallvirt(out var v); return v?.Name == "CollideCheck"; }))
                {
                    ic.Remove();
                    ic.EmitDelegate((Player self, Vector2 at) =>
                    {
                        Vector2 position = self.Position;
                        self.Position = at;
                        foreach (Entity entity in Engine.Scene.Tracker.Entities[typeof(DreamBlock)])
                        {
                            if (dreamblock_enabled(entity))
                            {
                                if (Collide.Check(self, entity))
                                {
                                    self.Position = position;
                                    return true;
                                }
                            }
                        }
                        self.Position = position;
                        return false;
                    });
                    return;
                }
            }
            ReverseHelperModule.failed_to_hook_reverse_dreamblock = true;
            Logger.Log(LogLevel.Error, "ReverseHelper", "Failed to load ReversedDreamBlock");
            try
            {
                Logger.Log(LogLevel.Error, "ReverseHelper", il.ToString());
            }
            catch (Exception)
            {

            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool dreamblock_enabled(Entity db)
        {
            return (db.Components.Get<ReversedDreamBlockComponent>() is null) == ReverseHelperModule.playerHasDreamDash;
        }

        private static void Player_DreamDashCheck(ILContext il)
        {
            ILCursor ic = new(il);
            if (ic.TryGotoNext(MoveType.After, (i) => { i.MatchCallvirt(out var v); return v?.Name == "get_Inventory"; },
                (i) => { i.MatchLdfld(out var v); return v?.Name == "DreamDash"; }))
            {
                ic.EmitDelegate((bool x) => true);

                if (ic.TryGotoNext(MoveType.Before, (i) => { i.MatchCall(out var v); return v?.Name == "CollideFirst"; }))
                {
                    ic.Remove();
                    ic.EmitDelegate((Player self, Vector2 at) =>
                    {
                        Vector2 position = self.Position;
                        self.Position = at;
                        DreamBlock db = null;
                        foreach (Entity item in self.Scene.Tracker.Entities[typeof(DreamBlock)])
                        {
                            if (Collide.Check(self, item))
                            {
                                //(item is DreamBlock i2);
                                {
                                    if (dreamblock_enabled(item))
                                    {
                                        //is_active
                                        db = item as DreamBlock;
                                        break;
                                    }
                                }
                            }
                        }
                        self.Position = position;
                        return db;
                    });

                    while (ic.TryGotoNext(MoveType.Before, (i) => { i.MatchCall(out var v); return v?.Name == "CollideCheck"; }))
                    {
                        ic.Remove();
                        ic.EmitDelegate((Player self, Vector2 at) =>
                        {
                            Vector2 position = self.Position;
                            self.Position = at;
                            foreach (Entity entity in Engine.Scene.Tracker.Entities[typeof(Solid)])
                            {
                                if (entity is DreamBlock db && dreamblock_enabled(db))
                                {
                                }
                                else
                                {
                                    if (Collide.Check(self, entity))
                                    {
                                        self.Position = position;
                                        return true;
                                    }
                                }
                            }
                            self.Position = position;
                            return false;
                        });

                    }
                    return;
                }
            }
            {
                ReverseHelperModule.failed_to_hook_reverse_dreamblock = true;
                Logger.Log(LogLevel.Error, "ReverseHelper", "Failed to load ReversedDreamBlock");
                try
                {
                    Logger.Log(LogLevel.Error, "ReverseHelper", ic.ToString());


                }
                catch (Exception)
                {

                }
            }
        }



        public static void Unload()
        {
            IL.Celeste.Player.DreamDashCheck -= Player_DreamDashCheck;
            //IL.Celeste.Player.DashCoroutine -= Player_DashCoroutine;
            dashcoroutine?.Dispose();
            grabbag_workaround?.Dispose();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (bindOnRoomStart)
            {
                foreach (Entity playerCollider in Scene.Tracker.Entities[typeof(DreamBlock)])
                {
                    if (playerCollider.Collider.Collide(Collider))
                    {
                        BindEntity(playerCollider);
                    }
                }
            }
            (scene as Level).OnEndOfFrame += DreamToggleListener.ForceUpdate;
            //(scene as Level).Background.Backdrop
        }

        private void BindEntity(Entity entity)
        {
            //if (entity is DreamBlock db)
            {
                entity.Add(new ReversedDreamBlockComponent());
            }
        }
    }
}