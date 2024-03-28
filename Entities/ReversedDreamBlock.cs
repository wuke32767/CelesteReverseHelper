using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Component = Monocle.Component;

namespace Celeste.Mod.ReverseHelper.Entities
{
    public class DreamBlockConfig() : Component(true, false)
    {
        public bool? reverse;
        public bool? enable;
        public bool? disable;
        public bool? highpriority;

        public DreamBlockConfig Reverse(bool? v = true)
        {
            if (v.HasValue)
            {
                reverse = v;
            }
            return this;
        }
        public DreamBlockConfig Enable(bool? v = true)
        {
            if (v.HasValue)
            {
                enable = v;
            }
            return this;
        }
        public DreamBlockConfig Disable(bool? v = true)
        {
            if (v.HasValue)
            {
                disable = v;
            }
            return this;
        }
        public DreamBlockConfig HighPriority(bool? v = true)
        {
            if (v.HasValue)
            {
                highpriority = v;
            }
            return this;
        }
        public static bool TryGet(Entity e, [MaybeNullWhen(false)] out DreamBlockConfig cfg)
        {
            if (e.Get<DreamBlockConfig>() is DreamBlockConfig c)
            {
                cfg = c;
                return true;
            }
            cfg = null;
            return false;
        }
        public static DreamBlockConfig Get(Entity e)
        {
            if (e.Get<DreamBlockConfig>() is DreamBlockConfig c)
            {
                return c;
            }
            e.Add(c = new DreamBlockConfig());
            return c;
        }

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
        bool? reverse = false;
        bool? alwaysEnable = false;
        bool? alwaysDisable = false;
        bool? highPriority = false;
        public override void Update()
        {
            base.Update();
        }
        public static MTexture? invalid_img;
        public ReversedDreamBlock(Vector2 position, float width, float height, bool? alwaysenable, bool? alw, bool? rev, bool? highPrior) : base(position)
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
            reverse = rev;
            highPriority = highPrior;
        }

        // Token: 0x06000EF3 RID: 3827 RVA: 0x0003A0EC File Offset: 0x000382EC
        public ReversedDreamBlock(EntityData e, Vector2 offset) : this(e.Position + offset, e.Width, e.Height, e.OptionalBool("alwaysEnable"), e.OptionalBool("alwaysDisable"), e.OptionalBool("reverse", true)/*why it's called ReversedDreamBlockContainer*/, e.OptionalBool("highPriority"))
        {
        }
        static ILHook dashcoroutine;
        public static void Load()
        {
            IL.Celeste.Player.DreamDashCheck += Player_DreamDashCheck;
            //IL.Celeste.Player.DashCoroutine += Player_DashCoroutine;
            var mi = typeof(Player).GetMethod("DashCoroutine", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget();
            dashcoroutine = new ILHook(mi, Player_DashCoroutine);


        }
        //static ILHook? communal_dreamtunnel;
        //static ILHook? grabbag_workaround;
        //static ILHook? grabbag_workaround_img_il;
        //static ILHook? grabbag_workaround_img_gt;
        //static ILHook? grabbag_workaround_img_rr;
        //static Hook? grabbag_workaround_img_on;
        //static bool grabbag_on = false;
        //static int grabbag_clear = 2;
        //static List<Entity> grabbag_list;
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //static bool grabbag_enabled(Entity e)
        //{
        //    return dreamblock_enabled(DreamSpinner.get_block(e));
        //}
        public static void LoadContent()
        {
            //if(DreamSpinnerRenderer.ReverseHelperSupported is null)
            //{
            //    //var com = ReverseHelperExtern.CommunalHelper.DreamTunnelEntry.Added;
            //    //if(com is not null)
            //    //{
            //    //    communal_dreamtunnel=new(com,(ILContext il)=>
            //    //    {
            //    //        ILCursor ic = new(il);
            //    //        if(ic.TryGotoNext(MoveType.After,i=>i.MatchLdfld<PlayerInventory>("DreamDash")))
            //    //        {
            //    //            ic.Emit(OpCodes.Pop);
            //    //            ic.Emit(OpCodes.Pop);
            //    //            ic.EmitDelegate()
            //    //        }
            //    //    });
            //    //}
            //    var upd = DreamSpinner.Update;
            //    if (upd is not null)
            //    {
            //        grabbag_workaround = new ILHook(upd, (ILContext il) =>
            //        {
            //            ILCursor ic = new ILCursor(il);
            //            ic.Emit(OpCodes.Ldarg_0);
            //            ic.EmitDelegate((Entity e) =>
            //                {
            //                    Color color;
            //                    color = Color.Black;
            //                    if (!grabbag_enabled(e))
            //                    {
            //                        color = new Color(25, 25, 25);
            //                    }
            //                    else
            //                    {
            //                        if (DreamSpinner.get_OneUse(e))
            //                        {
            //                            color = new Color(30, 22, 10);
            //                        }
            //                    }
            //                    DreamSpinner.set_color(e, color);
            //                });
            //            while (ic.TryGotoNext(MoveType.After, (i) => { i.MatchCallvirt(out var v); return v?.Name == "get_Inventory"; },
            //                (i) => { i.MatchLdfld(out var v); return v?.Name == "DreamDash"; }))
            //            {
            //                ic.Emit(OpCodes.Ldarg_0);
            //                ic.EmitDelegate((bool x, Entity self) => grabbag_enabled(self));
            //            }
            //        });
            //    }
            //    var ren = DreamSpinnerRenderer.BeforeRender;
            //    var get = DreamSpinnerRenderer.GetSpinnersToRender;
            //    var rren = DreamSpinnerRenderer.Render;
            //    if (ren is not null && get is not null && rren is not null)
            //    {
            //        grabbag_workaround_img_gt = new ILHook(get, (ILContext il) =>
            //        {
            //            ILCursor ic = new ILCursor(il);
            //            if (ic.TryGotoNext(MoveType.Before, (i) => { i.MatchCallOrCallvirt(out var v); return v?.Name == "InView"; }))
            //            {
            //                ic.Emit(OpCodes.Dup);
            //                ic.Index++;
            //                ic.EmitDelegate((Entity e, bool b) =>
            //                {
            //                    return b && (grabbag_enabled(e) == grabbag_on);
            //                });
            //            }
            //        });
            //        grabbag_workaround_img_il = new ILHook(ren, (ILContext il) =>
            //        {
            //            ILCursor ic = new ILCursor(il);
            //            if (ic.TryGotoNext(MoveType.After,
            //                (i) => { i.MatchLdflda(out var v); return v?.Name == "Inventory"; },
            //                (i) => { i.MatchLdfld(out var v); return v?.Name == "DreamDash"; }))
            //            {
            //                ic.EmitDelegate((bool b) =>
            //                {
            //                    return grabbag_on;
            //                });
            //                while (ic.TryGotoNext(MoveType.Before,
            //                (i) => { i.MatchCallOrCallvirt(out var v); return v?.Name == "Clear"; }))
            //                {
            //                    ic.Remove();
            //                    ic.EmitDelegate((GraphicsDevice geometrydash, Color c) =>
            //                    {
            //                        if (grabbag_clear > 0)
            //                        {
            //                            geometrydash.Clear(c);
            //                        }
            //                        grabbag_clear--;
            //                    });
            //                }
            //            }

            //        });
            //        grabbag_workaround_img_on = new Hook(ren, (Action<Entity> orig, Entity self) =>
            //        {
            //            //List<Entity> list = [];
            //            //List<Entity> list2 = [];
            //            //foreach (Entity entity in Engine.Scene.Tracker.Entities[DreamSpinner.Type])
            //            //{
            //            //    if (DreamSpinner.InView(entity))
            //            //    {
            //            //        list.Add(entity);
            //            //    }
            //            //    else
            //            //    {
            //            //        list2.Add(entity);
            //            //    }
            //            //}

            //            grabbag_clear = 2;
            //            grabbag_on = true;
            //            orig(self);
            //            grabbag_on = false;
            //            orig(self);
            //        });
            //        grabbag_workaround_img_rr = new ILHook(rren, (ILContext il) =>
            //        {
            //            ILCursor ic = new ILCursor(il);
            //            if (ic.TryGotoNext(MoveType.Before, (i) => i.MatchBrfalse(out _)))
            //            {
            //                ic.EmitDelegate((bool b) =>
            //                {
            //                    if (Engine.Scene.Tracker.Entities[DreamSpinner.Type].Count > 0)
            //                    {
            //                        return true;
            //                    }
            //                    return false;
            //                });
            //            }
            //        });
            //    }
            //}
        }
        static class Player_DashCoroutine_Helper
        {
            public static bool @true(bool x) => true;
            public static bool CollideCheck(Player self, Vector2 at)
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
                ic.EmitDelegate(Player_DashCoroutine_Helper.@true);

                if (ic.TryGotoNext(MoveType.Before, (i) => { i.MatchCallvirt(out var v); return v?.Name == "CollideCheck"; }))
                {
                    ic.Remove();

                    ic.EmitDelegate(Player_DashCoroutine_Helper.CollideCheck);
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
            var com = db.Components.Get<DreamBlockConfig>();
            if (com is not null && com.enable == true)
            {
                return true;
            }
            if (com is not null && com.disable == true)
            {
                return false;
            }
            bool has = com is not null && com.reverse == true;
            return has != ReverseHelperModule.playerHasDreamDash;
        }
        static class Player_DreamDashCheck_Helper
        {
            internal static DreamBlock CollideFirst(Player self, Vector2 at)
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
            }
            internal static bool CheckPriorityAndStore(DreamBlock t, Player p)
            {
                if (t is not null && t.Get<DreamBlockConfig>() is DreamBlockConfig cfg && (cfg.highpriority ?? false))
                {
                    p.dreamBlock = t;
                    return true;
                }
                return false;

            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool @true(bool x) => true;
            public static bool CollideCheck(Player self, Vector2 at)
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
            }
        }
        private static void Player_DreamDashCheck(ILContext il)
        {
            ILCursor ic = new(il);
            if (ic.TryGotoNext(MoveType.After, (i) => { i.MatchCallvirt(out var v); return v?.Name == "get_Inventory"; },
                (i) => { i.MatchLdfld(out var v); return v?.Name == "DreamDash"; }))
            {
                ic.EmitDelegate(Player_DreamDashCheck_Helper.@true);

                if (ic.TryGotoNext(MoveType.Before, (i) => { i.MatchCall(out var v); return v?.Name == "CollideFirst"; }))
                {
                    var label = ic.DefineLabel();

                    ic.Remove();
                    ic.EmitDelegate(Player_DreamDashCheck_Helper.CollideFirst);

                    ic.Emit(OpCodes.Dup);
                    ic.Emit(OpCodes.Ldarg_0);
                    ic.EmitDelegate(Player_DreamDashCheck_Helper.CheckPriorityAndStore);

                    ic.Emit(OpCodes.Brfalse, label);
                    ic.Emit(OpCodes.Pop);
                    ic.Emit(OpCodes.Ldc_I4_1);
                    ic.Emit(OpCodes.Ret);
                    ic.MarkLabel(label);

                    while (ic.TryGotoNext(MoveType.Before, (i) => { i.MatchCall(out var v); return v?.Name == "CollideCheck"; }))
                    {
                        ic.Remove();
                        ic.EmitDelegate(Player_DreamDashCheck_Helper.CollideCheck);
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
            //grabbag_workaround?.Dispose();
            //grabbag_workaround_img_il?.Dispose();
            //grabbag_workaround_img_on?.Dispose();
            //grabbag_workaround_img_gt?.Dispose();
            //grabbag_workaround_img_rr?.Dispose();
        }
        internal static Dictionary<Type, (Action<Entity> activate, Action<Entity> deactivate)> ExternalDreamBlockLike = [];
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (bindOnRoomStart)
            {
                //var com = ReverseHelperExtern.CommunalHelper.DreamTunnelEntry.Type;
                //if (com is not null)
                //{
                //    foreach (Entity playerCollider in Scene.Tracker.Entities[com])
                //    {
                //        if (playerCollider.Collider.Collide(Collider))
                //        {
                //            BindEntity(playerCollider);
                //        }
                //    }
                //}
                foreach (Entity playerCollider in Scene.Tracker.Entities[typeof(DreamBlock)])
                {
                    if (playerCollider.Collider.Collide(Collider))
                    {
                        BindEntity(playerCollider);
                    }
                }
                foreach (var (t, _) in ExternalDreamBlockLike)
                {
                    if (Scene.Tracker.Entities.TryGetValue(t, out var list))
                    {
                        foreach (Entity playerCollider in list)
                        {
                            if (playerCollider.Collider.Collide(Collider))
                            {
                                BindEntity(playerCollider);
                            }
                        }
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
                DreamBlockConfig.Get(entity)
                    .Reverse(reverse).Enable(alwaysEnable).Disable(alwaysDisable).HighPriority(highPriority);
            }
        }
    }
}