using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static Celeste.Mod.ReverseHelper.ReverseHelperExtern.IsaGrabBag;
using Component = Monocle.Component;

namespace Celeste.Mod.ReverseHelper.Entities
{
    public class ReversedDreamBlockComponent(bool enable, bool disable) : Component(true, false)
    {
        public bool enable = enable;
        public bool disable = disable;

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
        bool alwaysEnable = false;
        bool alwaysDisable = false;
        public override void Update()
        {
            base.Update();
        }
        public static MTexture? invalid_img;
        public ReversedDreamBlock(Vector2 position, float width, float height, bool alwaysenable, bool alw) : base(position)
        {
            alwaysEnable = alwaysenable;
            alwaysDisable = alw;
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
        public ReversedDreamBlock(EntityData e, Vector2 offset) : this(e.Position + offset, e.Width, e.Height, e.Bool("alwaysEnable", false), e.Bool("alwaysDisable", false))
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
        //static ILHook? communal_dreamtunnel;
        static ILHook? grabbag_workaround;
        static ILHook? grabbag_workaround_img_il;
        static ILHook? grabbag_workaround_img_gt;
        static ILHook? grabbag_workaround_img_rr;
        static Hook? grabbag_workaround_img_on;
        static bool grabbag_on = false;
        static int grabbag_clear = 2;
        //static List<Entity> grabbag_list;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool grabbag_enabled(Entity e)
        {
            return dreamblock_enabled(DreamSpinner.get_block(e));
        }
        public static void LoadContent()
        {
            //var com = ReverseHelperExtern.CommunalHelper.DreamTunnelEntry.Added;
            //if(com is not null)
            //{
            //    communal_dreamtunnel=new(com,(ILContext il)=>
            //    {
            //        ILCursor ic = new(il);
            //        if(ic.TryGotoNext(MoveType.After,i=>i.MatchLdfld<PlayerInventory>("DreamDash")))
            //        {
            //            ic.Emit(OpCodes.Pop);
            //            ic.Emit(OpCodes.Pop);
            //            ic.EmitDelegate()
            //        }
            //    });
            //}
            var upd = DreamSpinner.Update;
            if (upd is not null)
            {
                grabbag_workaround = new ILHook(upd, (ILContext il) =>
                {
                    ILCursor ic = new ILCursor(il);
                    ic.Emit(OpCodes.Ldarg_0);
                    ic.EmitDelegate((Entity e) =>
                        {
                            Color color;
                            color = Color.Black;
                            if (!grabbag_enabled(e))
                            {
                                color = new Color(25, 25, 25);
                            }
                            else
                            {
                                if (DreamSpinner.get_OneUse(e))
                                {
                                    color = new Color(30, 22, 10);
                                }
                            }
                            DreamSpinner.set_color(e, color);
                        });
                    while (ic.TryGotoNext(MoveType.After, (i) => { i.MatchCallvirt(out var v); return v?.Name == "get_Inventory"; },
                        (i) => { i.MatchLdfld(out var v); return v?.Name == "DreamDash"; }))
                    {
                        ic.Emit(OpCodes.Ldarg_0);
                        ic.EmitDelegate((bool x, Entity self) => grabbag_enabled(self));
                    }
                });
            }
            var ren = DreamSpinnerRenderer.BeforeRender;
            var get = DreamSpinnerRenderer.GetSpinnersToRender;
            var rren = DreamSpinnerRenderer.Render;
            if (ren is not null && get is not null && rren is not null)
            {
                grabbag_workaround_img_gt = new ILHook(get, (ILContext il) =>
                {
                    ILCursor ic = new ILCursor(il);
                    if (ic.TryGotoNext(MoveType.Before, (i) => { i.MatchCallOrCallvirt(out var v); return v?.Name == "InView"; }))
                    {
                        ic.Emit(OpCodes.Dup);
                        ic.Index++;
                        ic.EmitDelegate((Entity e, bool b) =>
                        {
                            return b && (grabbag_enabled(e) == grabbag_on);
                        });
                    }
                });
                grabbag_workaround_img_il = new ILHook(ren, (ILContext il) =>
                {
                    ILCursor ic = new ILCursor(il);
                    if (ic.TryGotoNext(MoveType.After,
                        (i) => { i.MatchLdflda(out var v); return v?.Name == "Inventory"; },
                        (i) => { i.MatchLdfld(out var v); return v?.Name == "DreamDash"; }))
                    {
                        ic.EmitDelegate((bool b) =>
                        {
                            return grabbag_on;
                        });
                        while (ic.TryGotoNext(MoveType.Before,
                        (i) => { i.MatchCallOrCallvirt(out var v); return v?.Name == "Clear"; }))
                        {
                            ic.Remove();
                            ic.EmitDelegate((GraphicsDevice geometrydash, Color c) =>
                            {
                                if (grabbag_clear > 0)
                                {
                                    geometrydash.Clear(c);
                                }
                                grabbag_clear--;
                            });
                        }
                    }

                });
                grabbag_workaround_img_on = new Hook(ren, (Action<Entity> orig, Entity self) =>
                {
                    //List<Entity> list = [];
                    //List<Entity> list2 = [];
                    //foreach (Entity entity in Engine.Scene.Tracker.Entities[DreamSpinner.Type])
                    //{
                    //    if (DreamSpinner.InView(entity))
                    //    {
                    //        list.Add(entity);
                    //    }
                    //    else
                    //    {
                    //        list2.Add(entity);
                    //    }
                    //}

                    grabbag_clear = 2;
                    grabbag_on = true;
                    orig(self);
                    grabbag_on = false;
                    orig(self);
                });
                grabbag_workaround_img_rr = new ILHook(rren, (ILContext il) =>
                {
                    ILCursor ic = new ILCursor(il);
                    if (ic.TryGotoNext(MoveType.Before, (i) => i.MatchBrfalse(out _)))
                    {
                        ic.EmitDelegate((bool b) =>
                        {
                            if (Engine.Scene.Tracker.Entities[DreamSpinner.Type].Count > 0)
                            {
                                return true;
                            }
                            return false;
                        });
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
            var com = db.Components.Get<ReversedDreamBlockComponent>();
            if (com is not null && com.enable == true)
            {
                return true;
            }
            if (com is not null && com.disable == true)
            {
                return false;
            }
            return (com is null) == ReverseHelperModule.playerHasDreamDash;
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
            grabbag_workaround_img_il?.Dispose();
            grabbag_workaround_img_on?.Dispose();
            grabbag_workaround_img_gt?.Dispose();
            grabbag_workaround_img_rr?.Dispose();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (bindOnRoomStart)
            {
                var com = ReverseHelperExtern.CommunalHelper.DreamTunnelEntry.Type;
                if (com is not null)
                {
                    foreach (Entity playerCollider in Scene.Tracker.Entities[com])
                    {
                        if (playerCollider.Collider.Collide(Collider))
                        {
                            BindEntity(playerCollider);
                        }
                    }
                }
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
            //if (entity is DreamBlock db || entity is not DreamBlock)
            {
                entity.Add(new ReversedDreamBlockComponent(alwaysEnable, alwaysDisable));
            }
        }
    }
}