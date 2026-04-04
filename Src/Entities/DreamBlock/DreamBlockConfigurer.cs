using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;
using MonoMod;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [Flags]
    public enum DreamBlockConfigFlags
    {
        reverse = 1 << 0,
        alwaysEnable = 1 << 1,
        alwaysDisable = 1 << 2,
        highPriority = 1 << 3,
        [WIP]
        ghostMode = 1 << 4,
        //[WIP("impossible now.")]
        //touchMode,
        useEntryAngle = 1 << 5,
        [WIP]
        ghostDisableCollidable = 1 << 6,
        exactMode = 1 << 7,
    }
    public class DreamBlockTrackers
    {
        public DreamBlockTrackers()
        {
            ByIndex = ImmutableArray.Create
            (Reverse, Enable, Disable, HighPriority, GhostMode, UseEntryAngle,
              GhostDisableCollidable, ExactMode);
        }
        public static ConditionalWeakTable<Scene, DreamBlockTrackers> Trackers = [];

        public static DreamBlockTrackers GetTracker(Scene? scene) => scene is null ? null! : Trackers.GetOrCreateValue(scene);
        public static DreamBlockTrackers? TryGetTracker(Scene scene)
        {
            if (!Trackers.TryGetValue(scene, out var d))
            {
                d = null;
            }
            return d;
        }

        public readonly List<Entity> Reverse = [];
        public readonly List<Entity> Enable = [];
        public readonly List<Entity> Disable = [];
        public readonly List<Entity> HighPriority = [];
        public readonly List<Entity> GhostMode = [];
        public readonly List<Entity> UseEntryAngle = [];
        public readonly List<Entity> GhostDisableCollidable = [];
        public readonly List<Entity> ExactMode = [];
        public readonly ImmutableArray<List<Entity>> ByIndex;

        //according to https://learn.microsoft.com/en-us/dotnet/api/system.enum.getvalues?view=net-8.0 #Remarks,
        //it's sorted
        public static readonly ImmutableArray<DreamBlockConfigFlags> valuelist = Enum.GetValues<DreamBlockConfigFlags>()/*.Order()*/.ToImmutableArray();
        public static readonly ImmutableArray<string> namelist = Enum.GetNames<DreamBlockConfigFlags>()/*.Order()*/.ToImmutableArray();
        public void TrackListed(DreamBlockConfig e, DreamBlockConfigFlags flag)
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

        public void UntrackListed(DreamBlockConfig e, DreamBlockConfigFlags flag)
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
        public void Clear()
        {
            foreach (var item in ByIndex)
            {
                item.Clear();
            }
        }
        public void Track(DreamBlockConfig e, DreamBlockConfigFlags flag)
        {
            if (e.Entity?.Scene is not null)
            {
                ByIndex[valuelist.BinarySearch(flag)].Add(e.Entity);
                e.tracked = true;
            }
        }

        public void Untrack(DreamBlockConfig e, DreamBlockConfigFlags flag)
        {
            if (e.Entity?.Scene is not null && e.tracked)
            {
                ByIndex[valuelist.BinarySearch(flag)].Remove(e.Entity);
            }
        }
        static object? sr;
        [SourceGen.Loader.Load]
        public static void Load()
        {
            //On.Celeste.Level.End += Level_End;
            static void saveState(Dictionary<Type, Dictionary<string, object>> d, Level l)
            {
                var dc = ReverseHelperExtern.SpeedRunTool_Interop.DeepClone ?? (x => x);
                if (TryGetTracker(l) is { } tracker)
                {
                    if (!d.TryGetValue(typeof(DreamBlockTrackers), out var dx))
                    {
                        dx = new();
                        d.Add(typeof(DreamBlockTrackers), dx);
                    }
                    foreach (var (k, i) in tracker.ByIndex.Zip(namelist))
                    {
                        dx.Add(i, k.Select(dc).Cast<Entity>().ToArray());
                    }
                }
            }
            static void loadState(Dictionary<Type, Dictionary<string, object>> d, Level l)
            {
                var dc = ReverseHelperExtern.SpeedRunTool_Interop.DeepClone ?? (x => x);
                if (d.TryGetValue(typeof(DreamBlockTrackers), out var dx))
                {
                    var tracker = GetTracker(l);
                    foreach (var (k, i) in tracker.ByIndex.Zip(namelist))
                    {
                        k.Clear();
                        k.AddRange(((Entity[])dx[i]).Select(dc).Cast<Entity>());
                    }
                }
            }
            static void clearCache(Level l)
            {
                foreach (DreamBlockConfig i in l.Tracker.GetComponents<DreamBlockConfig>())
                {
                    i.cache = null;
                }
            }
            sr = ReverseHelperExtern.SpeedRunTool_Interop.RegisterSaveLoadAction?.Invoke(saveState, loadState, null, clearCache, clearCache, null);
        }

        //private static void Level_End(On.Celeste.Level.orig_End orig, Level self)
        //{
        //    foreach (var item in ByIndex)
        //    {
        //        item.Clear();
        //    }
        //}


        [SourceGen.Loader.Unload]
        public static void Unload()
        {
            //On.Celeste.Level.End -= Level_End;
            if (sr is not null)
            {
                ReverseHelperExtern.SpeedRunTool_Interop.Unregister?.Invoke(sr);
            }
        }
    }
    [Tracked]
    public class DreamBlockConfig() : Component(true, false)
    {
        internal DreamBlockTrackers? cache;
        DreamBlockTrackers? Tracker => cache ?? (cache = DreamBlockTrackers.GetTracker(Scene));
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
                    Tracker?.Untrack(this, flags);
                }
                else
                {
                    Tracker?.Track(this, flags);
                }
            }
        }
        public bool? reverse { get => getter(DreamBlockConfigFlags.reverse); set => setter(DreamBlockConfigFlags.reverse, value); }
        public bool? enable { get => getter(DreamBlockConfigFlags.alwaysEnable); set => setter(DreamBlockConfigFlags.alwaysEnable, value); }
        public bool? disable { get => getter(DreamBlockConfigFlags.alwaysDisable); set => setter(DreamBlockConfigFlags.alwaysDisable, value); }
        public bool? highpriority { get => getter(DreamBlockConfigFlags.highPriority); set => setter(DreamBlockConfigFlags.highPriority, value); }
        //public bool? touchMode { get => getter(DreamBlockConfigFlags.touchMode); set => setter(DreamBlockConfigFlags.touchMode, value); }
        public bool? useEntryAngle { get => getter(DreamBlockConfigFlags.useEntryAngle); set => setter(DreamBlockConfigFlags.useEntryAngle, value); }
        public bool? ghostMode { get => getter(DreamBlockConfigFlags.ghostMode); set => setter(DreamBlockConfigFlags.ghostMode, value); }
        public bool? ghostDisableCollidable { get => getter(DreamBlockConfigFlags.ghostDisableCollidable); set => setter(DreamBlockConfigFlags.ghostDisableCollidable, value); }
        static DreamBlockConfig()
        {
            //OnScene<DreamBlockConfig>.OnSceneEnd = s => DreamBlockTrackers.Clear();
        }
        public DreamBlockConfigFlags withas => (has & state) | (flagasactive & ~has);
        public override void Added(Entity entity)
        {
            base.Added(entity);
            Tracker?.TrackListed(this, withas);
            PatchedAdded();
            cache = null;
        }
        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            Tracker?.UntrackListed(this, withas);
            PatchedRemoved();
            cache = null;
        }
        bool newAdded = false;
        void PatchedAdded()
        {
            void internals()
            {
                if (SceneAs<Level>() is { } level && !newAdded)
                {
                    level.NewDreamBlockCounter++;
                    newAdded = true;
                }
            }
            if (ReverseHelperModule.PatchInstalled)
            {
                internals();
            }
        }
        void PatchedRemoved()
        {
            void internals()
            {
                if (SceneAs<Level>() is { } level && newAdded)
                {
                    level.NewDreamBlockCounter--;
                    newAdded = false;
                }
            }
            if (ReverseHelperModule.PatchInstalled)
            {
                internals();
            }
        }
        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            Tracker?.TrackListed(this, withas);
            PatchedAdded();
            cache = null;
        }

        public override void EntityRemoved(Scene scene)
        {
            base.EntityRemoved(scene);
            Tracker?.UntrackListed(this, withas);
            PatchedRemoved();
            cache = null;
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
        static Hook? ondashcoroutine;
        static ILHook? ddcheck;
        static bool loaded = false;
        [SourceGen.Loader.LoadContent]
        public static void Load()
        {
            if (!loaded)
            {
                loaded = true;
                On.Celeste.Player.DreamDashCheck += Player_DreamDashCheck1;
                if (ReverseHelperModule.PatchInstalled)
                {
                    LoadPatched();
                }
                else
                {
                    //using var context = new DetourConfigContext(new DetourConfig("ReverseHelper", int.MinValue)).Use();
                    ddcheck = new ILHook(methodof<Player>(p => p.DreamDashCheck), Player_DreamDashCheckTwice);
                    //IL.Celeste.Player.DashCoroutine += Player_DashCoroutine;

                    var source = methodof<Player>(p => p.DashCoroutine).GetStateMachineTarget()!;
                    ondashcoroutine = new Hook(source, Player_DashCoroutine_MoveNext);

                    dashcoroutine = new ILHook(source!, Player_DashCoroutine);
                }
            }
        }
        static Hook? patch_activate;
        private static void UnloadPatched()
        {
            patch_activate?.Dispose();
            ddcheck?.Dispose();
            //IL.Celeste.Player.DreamDashCheck -= Player_DreamDashCheckV2;
        }
        private static void LoadPatched()
        {
            ddcheck = new ILHook(methodof<Player>(p => p.DreamDashCheck), Player_DreamDashCheck_Patched);

            patch_activate = new(propertyof((DreamBlock db) => db.Activated).GetGetMethod()!, DreamBlock_Activate);
            //IL.Celeste.Player.DreamDashCheck += Player_DreamDashCheckV2;
        }

        private static void Player_DreamDashCheck_Patched(ILContext il)
        {
            var ic = new ILCursor(il);
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
            var label = ic.DefineLabel();

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

        }

        private static void Player_DreamDashCheckV2(ILContext il)
        {

        }

        private static bool DreamBlock_Activate(Func<DreamBlock, bool> orig, DreamBlock self)
        {
            return dreamblock_enabledv2(self, orig(self));
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
            public static bool hasDreamDash(bool x, Player self) => Player_DreamDashCheck_Helper.HasDreamDash(x, self);
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
        static List<DreamBlock>? TrackerTemp;
        static readonly List<DreamBlock> TrackerPoolIGuess = [];
        [MakeFaster]
        private static void Player_DashCoroutine(ILContext il)
        {
            ILCursor ic = new(il);
            if (ic.TryGotoNext(MoveType.After, (i) => { i.MatchCallvirt(out var v); return v?.Name == "get_Inventory"; },
                (i) => { i.MatchLdfld(out var v); return v?.Name == "DreamDash"; }))
            {
                ic.EmitLdloc1();
                ic.EmitStaticLambda([MethodImpl(MethodImplOptions.AggressiveInlining)] (bool x, Player self) =>
                {
                    if (DreamBlockTrackers.TryGetTracker(self.Scene)?.ExactMode.Count > 0)
                    {
                        return Player_DashCoroutine_Helper.hasDreamDash(x, self);
                    }
                    else
                    {
                        static bool Extract(bool x, Player self)
                        {
                            var u = Vector2.UnitY;
                            if (ReverseHelperExtern.GravityHelper.IsPlayerInverted())
                            {
                                u = -u;
                            }
                            if (self.CollideFirst<DreamBlock>(self.Position + u) is { } d)
                            {
                                return dreamblock_enabledv2(d, x);
                            }
                            return x;
                        }
                        return Extract(x, self);
                    }
                }, "KeepCool");

                if (ic.TryGotoNext(MoveType.Before, (i) => { i.MatchCallvirt(out var v); return v?.Name == "CollideCheck"; }))
                {
                    VariableDefinition vars = new(il.Import(typeof(List<Entity>)));
                    il.Body.Variables.Add(vars);
                    ic.EmitLdloc1();
                    ic.EmitDelegate(EvilStart);
                    ic.EmitStloc(vars);
                    ic.Next = ic.Next!.Next;
                    ic.EmitLdloc(vars);
                    ic.EmitDelegate(EvilEndFaster);
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

        static List<Entity>? EvilStart(Player self)
        {
            if (DreamBlockTrackers.TryGetTracker(self.Scene)?.ExactMode.Count > 0)
            {
                TrackerPoolIGuess.Clear();
                TrackerTemp = TrackerPoolIGuess;
                var arr = self.Scene.Tracker.GetEntities<DreamBlock>();
                TrackerTemp.AddRange(arr.Cast<DreamBlock>());
                arr.RemoveAll(d => !dreamblock_enabled(d));
                return arr;
            }
            else
            {
                return null;
            }
        }

        static void EvilEndFaster(List<Entity> self)
        {
            if (TrackerTemp is not null)
            {
                var arr = self;
                arr.Clear();
                arr.AddRange(TrackerTemp);
                TrackerTemp.Clear();
                TrackerTemp = null;
            }
        }
        static void EvilEnd(Player self)
        {
            if (TrackerTemp is not null)
            {
                var arr = self.Scene.Tracker.GetEntities<DreamBlock>();
                EvilEndFaster(arr);
            }
        }
        private static bool Player_DashCoroutine_MoveNext(Func<object, bool> orig, object self)
        {
            try
            {
                return orig(self);
            }
            finally
            {
                if (TrackerTemp is not null)
                {
                    dynamic s = self;
                    Player p = s["<>4__this"];
                    EvilEnd(p);
                }
            }
        }

        public static bool dreamblockcheck_extracond(Entity db, Player p)
        {
            var com = db.Components.Get<DreamBlockConfig>();
            if (/*(com?.touchMode ?? false) ||*/ p.DashAttacking || p.StateMachine == Player.StDreamDash)
            {
                if (Player_DreamDashCheck_Helper.IsEntryAngleCorrect || (com?.useEntryAngle ?? false))
                {
                    return true;
                }
            }
            return false;
        }
        [Obsolete]
        public static bool dreamblock_enabled(Entity db)
        {
            if (ExternalDreamBlockDummy.TryGetValue(db.GetType(), out var dummy))
            {
                db = dummy(db);
            }
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
        public static bool dreamblock_enabledv2(Entity db, bool orig)
        {
            var com = db.Get<DreamBlockConfig>();
            return dreamblock_enabledv2(com, orig);
        }

        private static bool dreamblock_enabledv2(DreamBlockConfig? com, bool orig)
        {
            if (com is null)
            {
                return orig;
            }
            if (com.enable ?? false)
            {
                return true;
            }
            if (com.disable ?? false)
            {
                return false;
            }
            if (com.reverse ?? false)
            {
                return !orig;
            }
            return orig;
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
                    range = DreamBlockTrackers.GetTracker(self.Scene).Reverse.Concat(DreamBlockTrackers.GetTracker(self.Scene).Enable);
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
            public static bool HasDreamDash(bool x, Player self) => DreamBlockTrackers.GetTracker(self.Scene).Enable.Count > 0 || x || DreamBlockTrackers.GetTracker(self.Scene).Reverse.Count > 0;
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
            internal static bool HasEntryAngle2(float l, float r, Player self)
            {
                IsEntryAngleCorrect = l == r;
                return IsEntryAngleCorrect || DreamBlockTrackers.GetTracker(self.Scene).UseEntryAngle.Count > 0;
            }

            internal static bool HasDarkMatter(bool b, Player self)
            {
                return b /*|| DreamBlockTrackers.GetTracker(self.Scene).TouchMode.Count > 0*/;
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
            [MakeFaster]
            static void Ex(ILCursor ic)
            {
                ic.EmitStaticLambda(() => { }, "This method is greatly modified by me. Inform me before you ILHook it, to prevent hook collision.");
            }
            Ex(ic);

            if (!ic.TryGotoNext(MoveType.After, (i) => { i.MatchCallvirt(out var v); return v?.Name == "get_Inventory"; },
                (i) => { i.MatchLdfld(out var v); return v?.Name == "DreamDash"; }))
            {
                Exception();
                return;
            }

            ic.EmitLdarg0();
            ic.EmitDelegate(Player_DreamDashCheck_Helper.HasDreamDash);

            if (!ic.TryGotoNext(MoveType.After, i => { i.MatchCallvirt(out var v); return v?.Name == "get_DashAttacking"; }))
            {
                Exception();
                return;
            }
            //ic.EmitLdarg0();
            //ic.EmitDelegate(Player_DreamDashCheck_Helper.HasDarkMatter);

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
            ic.EmitLdarg0();
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

        struct State
        {
            internal bool exact;
            //internal bool failed;
            //internal bool success;
            internal DreamBlock? first;
            internal DreamBlockConfig? config;
            internal List<Entity?> multi;
        }
        [MakeFaster]
        private static void Player_DreamDashCheckTwice(ILContext il)
        {
            void Exception()
            {
                ReverseHelperModule.failed_to_hook_reverse_dreamblock = true;
                Logger.Log(LogLevel.Error, "ReverseHelper", "Failed to load DreamBlockConfigurer (Compat)");
                try
                {
                    Logger.Log(LogLevel.Error, "ReverseHelper", il.ToString());
                }
                catch (Exception)
                {
                }
            }
            ILCursor ic = new(il);
            ic.EmitStaticLambda([MethodImpl(MethodImplOptions.AggressiveInlining)] () => { }, "It's still too chaotic here. Please take care of this method.");
            var defs = new VariableDefinition(il.Import(typeof(State)));
            ic.Body.Variables.Add(defs);
            var pos = new VariableDefinition(il.Import(typeof(State)));
            ic.Body.Variables.Add(pos);
            ic.EmitLdarg0();
            ic.EmitLdarg1();
            ic.EmitStaticLambda((Player p, Vector2 root) =>
            {
                if (DreamBlockTrackers.TryGetTracker(p.Scene)?.ExactMode is { } a && a.Count > 0)
                {
                    var all = p.CollideAll<DreamBlock>(p.Position + root);
                    return new State()
                    {
                        exact = true,
                        multi = all,
                    };
                }
                else
                {
                    if (p.CollideFirst<DreamBlock>(p.Position + root) is { } d
                        && DreamBlockConfig.TryGet(d, out var cfg))
                    {
                        return new State() { exact = false, first = d, config = cfg, };
                    }
                }
                return default;
            }, "Precheck");
            ic.EmitStloc(defs);

            //ic.EmitStfld();
            if (!ic.TryGotoNext(MoveType.After, (i) => { i.MatchCallvirt(out var v); return v?.Name == "get_Inventory"; },
                (i) => { i.MatchLdfld(out var v); return v?.Name == "DreamDash"; }))
            {
                Exception();
                return;
            }

            ic.EmitLdloca(defs);
            ic.EmitStaticLambda((bool orig, ref State c) =>
            {
                static bool Get(bool orig, DreamBlockConfig? c)
                {
                    if (c is { })
                    {
                        if (c.enable ?? false)
                        {
                            return true;
                        }
                        else if (c.disable ?? false)
                        {
                            return false;
                        }
                        else if (c.reverse ?? false)
                        {
                            return !orig;
                        }
                    }
                    return orig;
                }
                bool r;
                if (c.exact)
                {
                    var span = CollectionsMarshal.AsSpan(c.multi);
                    foreach (ref var i in span)
                    {
                        if (DreamBlockConfig.TryGet(i!, out var cfg))
                        {
                            if (!Get(orig, cfg))
                            {
                                i = null;
                            }
                        }
                        else if (!orig)
                        {
                            i = null;
                        }
                    }
                    c.multi.RemoveAll(x => x is null);
                    r = c.multi.Count > 0;
                }
                else
                {
                    r = Get(orig, c.config);
                    //c.success = c.success && r;
                }
                return r;
            }, "CheckEnabled");

            if (!ic.TryGotoNext(MoveType.After, i => i.MatchCallvirt(out var v) && v.Name == "get_DashAttacking"))
            {
                Exception();
                return;
            }
            //ic.EmitLdarg0();
            //ic.EmitDelegate(Player_DreamDashCheck_Helper.HasDarkMatter);

            ILLabel? _tar = null;
            if (!ic.TryGotoNext(MoveType.Before, i => i.MatchBrfalse(out _)))
            {
                Exception();
                return;
            }
            if (!ic.TryGotoNext(MoveType.After, i => i.MatchBneUn(out _tar)))
            {
                Exception();
                return;
            }
            {
                ILLabel tar = _tar!;
                var ic2 = new ILCursor(il);
                ic2.GotoLabel(tar, MoveType.Before);
                ic2.EmitNop();
                var otar = il.DefineLabel();
                otar.Target = tar.Target;
                tar.Target = ic2.Prev;
                ic2.EmitLdloca(defs);
                ic2.EmitStaticLambda((ref State c) =>
                {
                    bool r;
                    if (c.exact)
                    {
                        var span = CollectionsMarshal.AsSpan(c.multi);
                        foreach (ref var i in span)
                        {
                            if (!DreamBlockConfig.TryGet(i!, out var cfg) || !(cfg.useEntryAngle ?? false))
                            {
                                i = null;
                            }
                        }
                        c.multi.RemoveAll(x => x is null);
                        r = c.multi.Count > 0;
                    }
                    else
                    {
                        r = c.config?.useEntryAngle ?? false;
                        //c.success = c.success && r;
                    }
                    return r;
                }, "CheckEntryAngle");
                var ne = ic.MarkLabel();
                ic2.EmitBrtrue(ne);
                ic2.EmitBr(otar);
                ic2.EmitNop();
            }

            if (!ic.TryGotoNext(MoveType.After, (i) => i.MatchCall(out var v) && v.Name == "CollideFirst"))
            {
                Exception();
                return;
            }
            ic.EmitLdloca(defs);
            ic.EmitLdarg0();
            ic.EmitStaticLambda((DreamBlock? db, ref State c, Player p) =>
            {
                if (c.exact)
                {
                    var r = c.multi.FirstOrDefault();
                    // ai-generated
                    // reviewed
                    static void ExceptInto<T>(List<T> A, List<T> B) where T : class?
                    {
                        int originalCount = B.Count;
                        int resultCount = A.Count - originalCount;

                        if (resultCount == 0)
                        {
                            B.Clear();
                            return;
                        }

                        B.EnsureCapacity(A.Count);
                        for (int i = 0; i < resultCount; i++)
                        {
                            B.Add(default!);
                        }
                        for (int i = originalCount - 1; i >= 0; i--)
                        {
                            B[resultCount + i] = B[i];
                        }

                        int j = resultCount;
                        int k = 0;

                        for (int i = 0; i < A.Count; i++)
                        {
                            if (j < B.Count && Object.ReferenceEquals(A[i], B[j]))
                            {
                                j++;
                            }
                            else
                            {
                                B[k] = A[i];
                                k++;
                            }
                        }

                        if (k < B.Count)
                        {
                            B.RemoveRange(k, B.Count - k);
                        }
                    }
                    ExceptInto(p.Scene.Tracker.GetEntities<DreamBlock>(), c.multi);
                    return r;
                }
                else
                {
                    //if (c.first != db && db != null)
                    //{
                    //    c.failed = true;
                    //}
                    return db;
                }
            }, "TryReplace");
            if (!ic.TryGotoNext(MoveType.After, i => i.MatchStloc0()))
            {
                Exception();
                return;
            }

            var label = il.DefineLabel();
            ic.Emit(OpCodes.Ldloc_0);
            //ic.Emit(OpCodes.Ldarg_0);
            ic.EmitStaticLambda((DreamBlock? t) =>
            {
                if (t is not null && DreamBlockConfig.TryGet(t, out var cfg) && (cfg.highpriority ?? false))
                {
                    return true;
                }
                return false;
            }, "CheckPriority");
            ic.Emit(OpCodes.Brtrue, label);

            FieldInfo value___ = fieldof((Player p) => p.dreamBlock);
            var target = ic.Clone();
            if (!target.TryGotoNext(
                //i => i.MatchLdarg(0),
                //i => i.MatchLdloc(0),
                i => i.MatchStfld(value___)
                //i => i.MatchLdcI4(1),
                //i => i.MatchRet()
                ))
            {
                Exception();
                return;
            }
            if (!target.TryGotoPrev(MoveType.Before,
                i => i.MatchLdarg(0)
                ))
            {
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
                //ic.Remove();
                //ic.EmitDelegate(Player_DreamDashCheck_Helper.CollideCheck);
                ic.EmitDup();
                ic.EmitStloc(pos);
                ic.Next = ic.Next!.Next;
                ic.EmitLdloca(defs);
                ic.EmitLdloc(pos);
                ic.EmitLdarg0();
                ic.EmitStaticLambda((bool success, ref State c, Vector2 pos, Player p) =>
                {
                    if (!success && c.exact)
                    {
                        return Collide.Check(p, c.multi, pos);
                    }
                    return success;
                }, "ExactlyWhatImDoing");
            }

        }



        [SourceGen.Loader.Unload]
        public static void Unload()
        {
            if (loaded)
            {
                On.Celeste.Player.DreamDashCheck -= Player_DreamDashCheck1;
                if (ReverseHelperModule.PatchInstalled)
                {
                    UnloadPatched();
                }
                else
                {
                    ddcheck?.Dispose();
                    //IL.Celeste.Player.DashCoroutine -= Player_DashCoroutine;
                    dashcoroutine?.Dispose();
                    //grabbag_workaround?.Dispose();
                    //grabbag_workaround_img_il?.Dispose();
                    //grabbag_workaround_img_on?.Dispose();
                    //grabbag_workaround_img_gt?.Dispose();
                    //grabbag_workaround_img_rr?.Dispose();
                    ondashcoroutine?.Dispose();
                }
                loaded = false;
            }
        }
        internal static Dictionary<Type, (Action<Entity> activate, Action<Entity> deactivate)> ExternalDreamBlockLike = [];
        internal static Dictionary<Type, Func<Entity, Entity>> ExternalDreamBlockDummy = [];

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
                foreach (var (t, g) in ExternalDreamBlockDummy)
                {
                    if (Scene.Tracker.Entities.TryGetValue(t, out var list))
                    {
                        foreach (Entity playerCollider in list)
                        {
                            if (playerCollider.Collider.Collide(Collider))
                            {
                                BindEntity(g(playerCollider));
                            }
                        }
                    }
                }
            }
            Scene.OnEndOfFrame += () => DreamToggleListener.ForceUpdate(Scene);
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