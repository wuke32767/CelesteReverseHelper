using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [Flags]
    public enum DreamBlockConfigFlags
    {
        reverse = 1 << 0,
        alwaysEnable = 1 << 1,
        alwaysDisable = 1 << 2,
        highPriority = 1 << 3,
        [WIP("should be used with other variants. will be wip before they are completed.")]
        touchMode = 1 << 4,
        useEntryAngle = 1 << 5,
        [WIP]
        ghostMode = 1 << 6,
        [WIP]
        ghostDisableCollidable = 1 << 7,
        //tunnelMode = 1 << 8,
        //disableThroughable = 1 << 9,
    }
    public static class DreamBlockTrackers
    {
        public static readonly List<Entity> Reverse = [];
        public static readonly List<Entity> Enable = [];
        public static readonly List<Entity> Disable = [];
        public static readonly List<Entity> HighPriority = [];
        public static readonly List<Entity> TouchMode = [];
        public static readonly List<Entity> UseEntryAngle = [];
        public static readonly List<Entity> GhostMode = [];
        public static readonly List<Entity> GhostDisableCollidable = [];
        public static readonly ImmutableArray<List<Entity>> ByIndex = ImmutableArray.Create
            (Reverse, Enable, Disable, HighPriority, TouchMode,
            UseEntryAngle, GhostMode, GhostDisableCollidable);
    
        //according to https://learn.microsoft.com/en-us/dotnet/api/system.enum.getvalues?view=net-8.0 #Remarks,
        //it's sorted
        public static readonly ImmutableArray<DreamBlockConfigFlags> valuelist = Enum.GetValues<DreamBlockConfigFlags>()/*.Order()*/.ToImmutableArray();
        public static readonly ImmutableArray<string> namelist = Enum.GetNames<DreamBlockConfigFlags>()/*.Order()*/.ToImmutableArray();
        public static void TrackListed(this DreamBlockConfig e, DreamBlockConfigFlags flag)
        {
            if (e.Entity?.Scene is not null && !e.tracked)
            {
                e.tracked = true;
                for (int i = 0; i < valuelist.Length; i++)
                {
                    var item = (DreamBlockConfigFlags)(1 << i);
                    if (flag.HasFlag(item))
                    {
                        ByIndex[i].Add(e.Entity);
                    }
                }
            }
        }

        public static void UntrackListed(this DreamBlockConfig e, DreamBlockConfigFlags flag)
        {
            if (e.Entity?.Scene is not null && e.tracked)
            {
                e.tracked = false;
                for (int i = 0; i < valuelist.Length; i++)
                {
                    var item = (DreamBlockConfigFlags)(1 << i);
                    if (flag.HasFlag(item))
                    {
                        ByIndex[i].Remove(e.Entity);
                    }
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
        public static void Track(this DreamBlockConfig e, DreamBlockConfigFlags flag)
        {
            if (e.Entity?.Scene is not null)
            {
                ByIndex[valuelist.BinarySearch(flag)].Add(e.Entity);
                e.tracked = true;
            }
        }

        public static void Untrack(this DreamBlockConfig e, DreamBlockConfigFlags flag)
        {
            if (e.Entity?.Scene is not null && e.tracked)
            {
                ByIndex[valuelist.BinarySearch(flag)].Remove(e.Entity);
            }
        }
        //[SourceGen.Loader.Load]
        //public static void Load()
        //{
        //    On.Celeste.Level.End += Level_End;
        //}
        //
        //private static void Level_End(On.Celeste.Level.orig_End orig, Level self)
        //{
        //    foreach (var item in ByIndex)
        //    {
        //        item.Clear();
        //    }
        //}
        //
        //
        //[SourceGen.Loader.Unload]
        //public static void Unload()
        //{
        //    On.Celeste.Level.End -= Level_End;
        //}
    }
    public class DreamBlockConfig() : Component(true, false)
    {
        //static int hacky= SingleGlobalEntity<DreamBlockConfig>.Register(s=>DreamBlockTrackers.Clear());
        public bool tracked = false;
        DreamBlockConfigFlags state;
        DreamBlockConfigFlags has;
        public const DreamBlockConfigFlags flagasactive = 0;
        public bool getteras(DreamBlockConfigFlags flags) => getter(flags) ?? flagasactive.HasFlag(flags);
        public bool? getter(DreamBlockConfigFlags flags) => has.HasFlag(flags) ? state.HasFlag(flags) : null;
        public void setter(DreamBlockConfigFlags flags, bool? value)
        {
            var orig = getteras(flags);

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

            if (orig != getteras(flags))
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
        }
        public bool? reverse { get => getter(DreamBlockConfigFlags.reverse); set => setter(DreamBlockConfigFlags.reverse, value); }
        public bool? enable { get => getter(DreamBlockConfigFlags.alwaysEnable); set => setter(DreamBlockConfigFlags.alwaysEnable, value); }
        public bool? disable { get => getter(DreamBlockConfigFlags.alwaysDisable); set => setter(DreamBlockConfigFlags.alwaysDisable, value); }
        public bool? highpriority { get => getter(DreamBlockConfigFlags.highPriority); set => setter(DreamBlockConfigFlags.highPriority, value); }
        public bool? touchMode { get => getter(DreamBlockConfigFlags.touchMode); set => setter(DreamBlockConfigFlags.touchMode, value); }
        public bool? useEntryAngle { get => getter(DreamBlockConfigFlags.useEntryAngle); set => setter(DreamBlockConfigFlags.useEntryAngle, value); }
        public bool? ghostMode { get => getter(DreamBlockConfigFlags.ghostMode); set => setter(DreamBlockConfigFlags.ghostMode, value); }
        public bool? ghostDisableCollidable { get => getter(DreamBlockConfigFlags.ghostDisableCollidable); set => setter(DreamBlockConfigFlags.ghostDisableCollidable, value); }
        [SourceGen.Loader.Load]
        public static void Load()
        {
            OnScene<DreamBlockConfig>.OnSceneEnd = s => DreamBlockTrackers.Clear();
        }
        public DreamBlockConfigFlags withas => (has & state) | (flagasactive & ~has);
        public override void Added(Entity entity)
        {
            this.TrackListed(withas);
            base.Added(entity);
            OnScene<DreamBlockConfig>.Construct(Entity.Scene);
        }
        public override void Removed(Entity entity)
        {
            this.UntrackListed(withas);
            base.Removed(entity);
        }
        public override void EntityAdded(Scene scene)
        {
            this.TrackListed(withas);
            base.EntityAdded(scene);
        }

        public override void EntityRemoved(Scene scene)
        {
            this.UntrackListed(withas);
            base.EntityRemoved(scene);
        }
        public override void Update()
        {
            base.Update();
            if (ghostMode is { } any)
            {
                if (ghostDisableCollidable ?? false)
                {
                    any = !DreamBlockConfigurer.dreamblock_enabled(Entity);
                }
                Entity.Collidable = any;
            }
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


    [CustomEntity($"ReverseHelper/ReversedDreamBlockContainer={nameof(Legacy)}", "ReverseHelper/DreamBlockConfigurer")]
    public class DreamBlockConfigurer(EntityData e, Vector2 offset) : Entity(e.Position + offset)
    {
        public static Entity Legacy(Level level, LevelData levelData, Vector2 offset, EntityData data)
        {
            if (!data.Values.TryGetValue("reverse", out _))
            {
                data.Values["reverse"] = true;
            }
            return new DreamBlockConfigurer(data, offset);
        }
        public new Level Scene => SceneAs<Level>();
        private bool bindOnRoomStart = true;
        //private FlagMatch flagBind;
        //private FlagMatch flagEnable;
        //private TypeMatch wraptype;
        //private float pushForce;
        //bool? reverse = e.OptionalBool("reverse", true);
        //bool? alwaysEnable = e.OptionalBool("alwaysEnable");
        //bool? alwaysDisable = e.OptionalBool("alwaysDisable");
        //bool? highPriority = e.OptionalBool("highPriority");
        //bool? touchMode = e.OptionalBool("touchMode");
        public override void Update()
        {
            base.Update();
        }
        public static Lazy<MTexture?> invalid_img = new(() => GFX.Game.GetAtlasSubtextures("__fallback")?.FirstOrDefault());
        public override void Added(Scene scene)
        {
            Visible = ReverseHelperModule.failed_to_hook_reverse_dreamblock;
            Depth = -200000;
            Collider = new Hitbox(e.Width, e.Height, 0f, 0f);

            base.Added(scene);
        }

        // Token: 0x06000EF3 RID: 3827 RVA: 0x0003A0EC File Offset: 0x000382EC
        //public DreamBlockConfigurer(EntityData e, Vector2 offset) : this(e.Position + offset, e.Width, e.Height,
        //    e.OptionalBool("alwaysEnable"),
        //    e.OptionalBool("alwaysDisable"),
        //    e.OptionalBool("reverse", true)/*this is why it's called ReversedDreamBlockContainer*/,
        //    e.OptionalBool("highPriority"),
        //    e.OptionalBool("touchMode"))
        //{
        //}
        static ILHook? dashcoroutine;
        [SourceGen.Loader.Load]
        public static void Load()
        {
            IL.Celeste.Player.DreamDashCheck += Player_DreamDashCheck;
            On.Celeste.Player.DreamDashCheck += Player_DreamDashCheck1;
            //IL.Celeste.Player.DashCoroutine += Player_DashCoroutine;
            dashcoroutine = new ILHook(methodof<Player>(p => p.DashCoroutine).GetStateMachineTarget()!, Player_DashCoroutine);
        }

        private static bool Player_DreamDashCheck1(On.Celeste.Player.orig_DreamDashCheck orig, Player self, Vector2 dir)
        {
            if (orig(self, dir))
            {
                if (self.dreamBlock.Get<DreamBlockConfig>()?.useEntryAngle ?? false)
                {
                    var dir2 = self.Speed;
                    self.DashDir = dir2.SafeNormalize();
                }
                return true;
            }
            return false;
        }

        static class Player_DashCoroutine_Helper
        {
            public static bool hasDreamDash(bool x) => true;
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
            invalid_img.Value?.Draw(Position);
            //base.Render();
        }
        private static void Player_DashCoroutine(ILContext il)
        {
            ILCursor ic = new(il);
            if (ic.TryGotoNext(MoveType.After, (i) => { i.MatchCallvirt(out var v); return v?.Name == "get_Inventory"; },
                (i) => { i.MatchLdfld(out var v); return v?.Name == "DreamDash"; }))
            {
                ic.EmitDelegate(Player_DashCoroutine_Helper.hasDreamDash);

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

        public static bool dreamblockcheck_extracond(Entity db, Player p)
        {
            var com = db.Components.Get<DreamBlockConfig>();
            if ((com?.touchMode ?? false) || p.DashAttacking || p.StateMachine == Player.StDreamDash)
            {
                if (Player_DreamDashCheck_Helper.IsEntryAngleCorrect || (com?.useEntryAngle ?? false))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool dreamblock_enabled(Entity db)
        {
            var com = db.Components.Get<DreamBlockConfig>();
            if (com?.enable ?? false)
            {
                return true;
            }
            if (com?.disable ?? false)
            {
                return false;
            }
            bool has = com?.reverse ?? false;
            return has != ReverseHelperModule.playerHasDreamDashBetter(db);
        }
        static class Player_DreamDashCheck_Helper
        {
            internal static bool IsEntryAngleCorrect = false;
            internal static DreamBlock? CollideFirst(Player self, Vector2 at)
            {
                Vector2 position = self.Position;
                self.Position = at;
                DreamBlock? db = null;
                IEnumerable<Entity> range;
                if (ReverseHelperModule.playerHasDreamDashBetter(self))
                {
                    range = self.Scene.Tracker.Entities[typeof(DreamBlock)].Where(dreamblock_enabled);
                }
                else
                {
                    range = DreamBlockTrackers.Reverse.Concat(DreamBlockTrackers.Enable);
                }
                foreach (Entity item in range)
                {
                    if (Collide.Check(self, item))
                    {
                        //(item is DreamBlock i2);
                        {
                            //if (dreamblock_enabled(item))
                            {
                                if (dreamblockcheck_extracond(item, self))
                                {
                                    //is_active
                                    db = item as DreamBlock;
                                    break;
                                }
                            }
                        }
                    }
                }
                self.Position = position;
                return db;
            }
            internal static bool CheckPriority(DreamBlock? t)
            {
                if (t is not null && DreamBlockConfig.TryGet(t, out var cfg) && (cfg.highpriority ?? false))
                {
                    //p.dreamBlock = t;
                    return true;
                }
                return false;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool HasDreamDash(bool x) => DreamBlockTrackers.Enable.Count > 0 || x || DreamBlockTrackers.Reverse.Count > 0;
            public static bool CollideCheck(Player self, Vector2 at)
            {
                Vector2 position = self.Position;
                self.Position = at;
                foreach (Entity entity in Engine.Scene.Tracker.Entities[typeof(Solid)])
                {
                    if (entity is DreamBlock db && dreamblock_enabled(db) && dreamblockcheck_extracond(db, self))
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
            internal static bool HasEntryAngle(float l, float r)
            {
                return IsEntryAngleCorrect = l == r;
            }
            internal static bool HasEntryAngle2(float l, float r)
            {
                IsEntryAngleCorrect = l == r;
                return IsEntryAngleCorrect || DreamBlockTrackers.UseEntryAngle.Count > 0;
            }

            internal static bool HasDarkMatter(bool b)
            {
                return b || DreamBlockTrackers.TouchMode.Count > 0;
            }
        }
        private static void Player_DreamDashCheck(ILContext il)
        {
            void Exception()
            {
                ReverseHelperModule.failed_to_hook_reverse_dreamblock = true;
                Logger.Log(LogLevel.Error, "ReverseHelper", "Failed to load DreamBlockConfigurer");
                try
                {
                    Logger.Log(LogLevel.Error, "ReverseHelper", il.ToString());
                }
                catch (Exception)
                {
                }
            }
            ILCursor ic = new(il);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void ThisMethodIsGreatlyModifiedByMe_InformMeBeforeYouILHookIt_ToPreventHookCollision() { }
            ic.EmitDelegate(ThisMethodIsGreatlyModifiedByMe_InformMeBeforeYouILHookIt_ToPreventHookCollision);

            if (!ic.TryGotoNext(MoveType.After, (i) => { i.MatchCallvirt(out var v); return v?.Name == "get_Inventory"; },
                (i) => { i.MatchLdfld(out var v); return v?.Name == "DreamDash"; }))
            {
                Exception();
                return;
            }

            ic.EmitDelegate(Player_DreamDashCheck_Helper.HasDreamDash);

            if (!ic.TryGotoNext(MoveType.After, i => { i.MatchCallvirt(out var v); return v?.Name == "get_DashAttacking"; }))
            {
                Exception();
                return;
            }
            ic.EmitDelegate(Player_DreamDashCheck_Helper.HasDarkMatter);

            if (!ic.TryGotoNext(MoveType.Before, i => i.MatchBeq(out _)))
            {
                Exception();
                return;
            }
            ic.EmitDelegate(Player_DreamDashCheck_Helper.HasEntryAngle);
            ic.EmitLdcI4(1);
            if (!ic.TryGotoNext(MoveType.Before, i => i.MatchBneUn(out _)))
            {
                Exception();
                return;
            }
            ic.EmitDelegate(Player_DreamDashCheck_Helper.HasEntryAngle2);
            ic.EmitLdcI4(1);

            if (!ic.TryGotoNext(MoveType.Before, (i) => { i.MatchCall(out var v); return v?.Name == "CollideFirst"; }))
            {
                Exception();
                return;
            }

            var label = ic.DefineLabel();

            ic.Remove();
            ic.EmitDelegate(Player_DreamDashCheck_Helper.CollideFirst);

            if (!ic.TryGotoNext(MoveType.After, i => i.MatchStloc0()))
            {
                Exception();
                return;
            }

            ic.Emit(OpCodes.Ldloc_0);
            //ic.Emit(OpCodes.Ldarg_0);
            ic.EmitDelegate(Player_DreamDashCheck_Helper.CheckPriority);

            FieldInfo value___ = fieldof((Player p) => p.dreamBlock);
            ic.Emit(OpCodes.Brtrue, label);
            var target = ic.Clone();
            if (!target.TryGotoNext(
                //i => i.MatchLdarg(0),
                //i => i.MatchLdloc(0),
                i => i.MatchStfld(value___)
                //i => i.MatchLdcI4(1),
                //i => i.MatchRet()
                ))
            {
                ic.MarkLabel();
                Exception();
                return;
            }
            if (!target.TryGotoPrev(MoveType.Before,
                i => i.MatchLdarg(0)
                ))
            {
                ic.MarkLabel();
                Exception();
                return;
            }
            target.MarkLabel(label);
            //ic.Emit(OpCodes.Pop);
            //ic.Emit(OpCodes.Ldc_I4_1);
            //ic.Emit(OpCodes.Ret);
            //ic.MarkLabel(label);

            while (ic.TryGotoNext(MoveType.Before, (i) => { i.MatchCall(out var v); return v?.Name == "CollideCheck"; }))
            {
                ic.Remove();
                ic.EmitDelegate(Player_DreamDashCheck_Helper.CollideCheck);
            }

        }



        [SourceGen.Loader.Unload]
        public static void Unload()
        {
            IL.Celeste.Player.DreamDashCheck -= Player_DreamDashCheck;
            On.Celeste.Player.DreamDashCheck -= Player_DreamDashCheck1;
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
            Scene.OnEndOfFrame += ()=>DreamToggleListener.ForceUpdate(null);
            //(scene as Level).Background.Backdrop
        }

        private void BindEntity(Entity entity)
        {
            //if (entity is DreamBlock db || entity is not DreamBlock)
            {
                var cfg = DreamBlockConfig.GetOrAdd(entity);
                foreach (var (n, i) in DreamBlockTrackers.namelist.Zip(DreamBlockTrackers.valuelist))
                {
                    var v = e.OptionalBool(n);
                    if (v is not null)
                    {
                        cfg.setter(i, v);
                    }
                }
            }
        }
    }
}