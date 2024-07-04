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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Component = Monocle.Component;

namespace Celeste.Mod.ReverseHelper.Entities
{
    public static class DreamBlockTrackers
    {
        public static readonly List<Entity> Reverse = [];
        public static readonly List<Entity> Enable = [];
        public static readonly List<Entity> Disable = [];
        public static readonly List<Entity> HighPriority = [];
        public static readonly List<List<Entity>> ByIndex = [Reverse, Enable, Disable, HighPriority];
        static ConditionalWeakTable<DreamBlockConfig, Action> Suspend = [];

        //according to https://learn.microsoft.com/en-us/dotnet/api/system.enum.getvalues?view=net-8.0 #Remarks,
        //it's sorted
        static List<DreamBlockConfigFlags> i_even_dont_know_whats_this = Enum.GetValues<DreamBlockConfigFlags>()/*.Order()*/.ToList();
        public static void TrackListed(this DreamBlockConfig e, DreamBlockConfigFlags flag, bool forceSuspend = false)
        {
            foreach (var item in i_even_dont_know_whats_this)
            {
                if (flag.HasFlag(item))
                {
                    Track(e, item, forceSuspend);
                }
            }
        }

        public static void Unsuspend(this DreamBlockConfig e) => Suspend.Remove(e);
        public static void UntrackListed(this DreamBlockConfig e, DreamBlockConfigFlags flag)
        {
            foreach (var item in i_even_dont_know_whats_this)
            {
                if (flag.HasFlag(item))
                {
                    Untrack(e, item);
                }
            }
        }
        public static void Clear()
        {
            foreach (var item in ByIndex)
            {
                item.Clear();
            }
        }
        public static void Track(this DreamBlockConfig e, DreamBlockConfigFlags flag, bool forceSuspend = false)
        {
            PushEvent(e, () =>
            {
                ByIndex[i_even_dont_know_whats_this.BinarySearch(flag)].Add(e.Entity);
            }, forceSuspend);
        }

        private static void PushEvent(DreamBlockConfig e, Action act, bool forceSuspend = false)
        {
            if (e.Entity?.Scene is not null && !forceSuspend)
            {
                act();
            }
            else
            {
                Suspend.TryGetValue(e, out var list);
                list += act;
                Suspend.AddOrUpdate(e, act);
            }
        }

        //Try move to Tracker
        public static void Continue(this DreamBlockConfig e)
        {
            if (e.Entity?.Scene is not null)
            {
                if (Suspend.TryGetValue(e, out var list))
                {
                    list();
                }
                Suspend.Remove(e);
            }
        }

        public static void Untrack(this DreamBlockConfig e, DreamBlockConfigFlags flag)
        {
            PushEvent(e, () =>
            {
                ByIndex[i_even_dont_know_whats_this.BinarySearch(flag)].Remove(e.Entity);
            });
        }
        [SourceGen.Loader.Load]
        public static void Load()
        {
            On.Celeste.Level.End += Level_End;
        }

        private static void Level_End(On.Celeste.Level.orig_End orig, Level self)
        {
            foreach (var item in ByIndex)
            {
                item.Clear();
            }
        }


        [SourceGen.Loader.Unload]
        public static void Unload()
        {
            On.Celeste.Level.End -= Level_End;
        }
    }
    [Flags]
    public enum DreamBlockConfigFlags
    {
        reverse = 1 << 0,
        enable = 1 << 1,
        disable = 1 << 2,
        highpriority = 1 << 3,
        //touchMode = 1 << 4,
        //touchModeUseEntryAngle = 1 << 5,
        //ghostMode = 1 << 6,
        //ghostDisableCollidable = 1 << 7,
        //tunnelMode = 1 << 8,
        //disableThroughable = 1 << 9,
    }
    /// <summary>
    /// There's a hidden constraint.
    /// these states are valid:<br />
    ///     Has no flag and not Suspending;<br />
    ///     Has no flag and Suspending but does nothing;<br />
    ///     Suspending;<br />
    ///     Tracked.<br />
    /// can not both in Suspend and in Trackers, but it is not hidden.
    /// </summary>
    public class DreamBlockConfig() : Component(true, false)
    {
        //static int hacky= SingleGlobalEntity<DreamBlockConfig>.Register(s=>DreamBlockTrackers.Clear());

        DreamBlockConfigFlags state;
        DreamBlockConfigFlags has;
        public bool? getter(DreamBlockConfigFlags flags) => has.HasFlag(flags) ? state.HasFlag(flags) : null;
        public void setter(DreamBlockConfigFlags flags, bool? value, bool @as = false)
        {
            var orig = getter(flags) ?? @as;
            if (orig != (value ?? @as))
            {
                if (orig)
                {
                    this.Untrack(flags);
                }
                else
                {
                    this.Track(flags);
                }
            }
            if (value is null)
            {
                has &= ~flags;
            }
            else
            {
                has |= flags;
                if (value.Value)
                {
                    state |= flags;
                }
                else
                {
                    state &= ~flags;
                }
            }
        }
        public bool? reverse { get => getter(DreamBlockConfigFlags.reverse); private set => setter(DreamBlockConfigFlags.reverse, value); }
        public bool? enable { get => getter(DreamBlockConfigFlags.enable); private set => setter(DreamBlockConfigFlags.enable, value); }
        public bool? disable { get => getter(DreamBlockConfigFlags.disable); private set => setter(DreamBlockConfigFlags.disable, value); }
        public bool? highpriority { get => getter(DreamBlockConfigFlags.highpriority); private set => setter(DreamBlockConfigFlags.highpriority, value); }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            this.Continue();
            SingleGlobalEntity<DreamBlockConfig>.Construct(Entity.Scene);
        }
        public override void Removed(Entity entity)
        {
            this.UntrackListed(has & state);
            this.Unsuspend();
            base.Removed(entity);
            DreamBlockTrackers.TrackListed(this, has & state, true);
        }
        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            this.Continue();
            //SingleGlobalEntity<DreamBlockConfig>.Construct(Entity.Scene);
        }

        public override void EntityRemoved(Scene scene)
        {
            this.UntrackListed(has & state);
            this.Unsuspend();
            base.EntityRemoved(scene);
            //DreamBlockTrackers.TrackListed(this, has & state, true);
        }
        //public override void SceneEnd(Scene scene)
        //{
        //    this.UntrackListed(has & state);
        //    this.Unsuspend();
        //    base.SceneEnd(scene);
        //    DreamBlockTrackers.TrackListed(this, has & state, true);// is there anyone would share it between scene?
        //}
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
        public static DreamBlockConfig GetOrAdd(Entity e)
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
    public class DreamBlockConfigurer : Entity
    {

        public new Level Scene => SceneAs<Level>();
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
        public DreamBlockConfigurer(Vector2 position, float width, float height, bool? alwaysenable, bool? alw, bool? rev, bool? highPrior) : base(position)
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
        public DreamBlockConfigurer(EntityData e, Vector2 offset) : this(e.Position + offset, e.Width, e.Height, e.OptionalBool("alwaysEnable"), e.OptionalBool("alwaysDisable"), e.OptionalBool("reverse", true)/*why it's called ReversedDreamBlockContainer*/, e.OptionalBool("highPriority"))
        {
        }
        static ILHook? dashcoroutine;
        [SourceGen.Loader.Load]
        public static void Load()
        {
            IL.Celeste.Player.DreamDashCheck += Player_DreamDashCheck;
            //IL.Celeste.Player.DashCoroutine += Player_DashCoroutine;
            dashcoroutine = new ILHook(typeof(Player).GetMethod("DashCoroutine", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetStateMachineTarget()!, Player_DashCoroutine);
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
            return has != ReverseHelperModule.playerHasDreamDashBetter(db);
        }
        static class Player_DreamDashCheck_Helper
        {
            internal static DreamBlock? CollideFirst(Player self, Vector2 at)
            {
                Vector2 position = self.Position;
                self.Position = at;
                DreamBlock? db = null;
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
            public static bool HasDreamDash(bool x) => true;
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
                ic.EmitDelegate(Player_DreamDashCheck_Helper.HasDreamDash);

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
                    Logger.Log(LogLevel.Error, "ReverseHelper", il.ToString());


                }
                catch (Exception)
                {

                }
            }
        }



        [SourceGen.Loader.Unload]
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
                foreach (Entity playerCollider in this.CollidableAll<DreamBlock>())
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
            Scene.OnEndOfFrame += DreamToggleListener.ForceUpdate;
            //(scene as Level).Background.Backdrop
        }

        private void BindEntity(Entity entity)
        {
            //if (entity is DreamBlock db || entity is not DreamBlock)
            {
                DreamBlockConfig.GetOrAdd(entity)
                    .Reverse(reverse).Enable(alwaysEnable).Disable(alwaysDisable).HighPriority(highPriority);
            }
        }
    }
}