using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [Tracked]
    [SourceGen.Loader.Preload("ReverseHelper/BarrierInteropHelper")]
    public partial class CustomInvisibleBarrierManager : Entity
    {
        bool working = false;
        bool actorsolid;
        bool jump;
        bool mod;
        bool jelly;

        public CustomInvisibleBarrierManager(bool actorsolid = true, bool jump = true, bool mod = true,
            bool jelly = false) : base()
        {
            AddTag(Tags.Global);
            this.actorsolid = actorsolid;
            this.jump = jump;
            this.mod = mod;
            this.jelly = jelly;
        }
        static ILHook? AttachedJumpThru;
        public override void SceneBegin(Scene scene)
        {
            base.SceneBegin(scene);
            Load();
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Load();
        }

        private void Load()
        {
            if (Scene.Tracker.GetEntity<CustomInvisibleBarrierManager>() != this)
            {
                RemoveSelf();
                return;
            }
            if (working == true)
            {
                return;
            }
            working = true;
            if (actorsolid)
            {
                IL.Celeste.Solid.MoveHExact += UniversalBarrier;
                IL.Celeste.Solid.MoveVExact += UniversalBarrier;
            }
            if (jump)
            {
                IL.Celeste.JumpThru.MoveHExact += UniversalBarrier;
                IL.Celeste.JumpThru.MoveVExact += UniversalBarrier;
            }
            if (mod)
            {
                var mov = ReverseHelperExtern.VortexHelperModule.AttachedJumpThru.MoveHExact;
                if (mov is not null && AttachedJumpThru is null)
                {
                    AttachedJumpThru = new ILHook(mov, UniversalBarrier);
                }
            }
            if (jelly)
            {
                On.Celeste.Holdable.Release += Holdable_Release;
            }
        }

        private void Holdable_Release(On.Celeste.Holdable.orig_Release orig, Holdable self, Vector2 force)
        {
            Guardian(() => orig(self, force), self.Entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Guardian(Action act, Entity self)
        {
            var tar = Engine.Scene.Tracker.Entities[typeof(CustomInvisibleBarrier)].Cast<CustomInvisibleBarrier>();
            foreach (var r in tar)
            {
                r.ApplyTo(self);
            }
            act();
            foreach (var r in tar)
            {
                r.Restore();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T Guardian<T>(Func<T> act, Entity self)
        {
            var tar = Engine.Scene.Tracker.Entities[typeof(CustomInvisibleBarrier)].Cast<CustomInvisibleBarrier>();
            foreach (var r in tar)
            {
                r.ApplyTo(self);
            }
            var ret = act();
            foreach (var r in tar)
            {
                r.Restore();
            }
            return ret;
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            RemoveSelf();
            RemoveHook();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            RemoveHook();
        }

        private void RemoveHook()
        {
            if (working == false)
            {
                return;
            }
            if (actorsolid)
            {
                IL.Celeste.Solid.MoveHExact -= UniversalBarrier;
                IL.Celeste.Solid.MoveVExact -= UniversalBarrier;
            }
            if (jump)
            {
                IL.Celeste.JumpThru.MoveHExact -= UniversalBarrier;
                IL.Celeste.JumpThru.MoveVExact -= UniversalBarrier;
            }
            if (mod)
            {
                AttachedJumpThru?.Dispose();
                AttachedJumpThru = null;
            }
            if (jelly)
            {
                On.Celeste.Holdable.Release -= Holdable_Release;
            }

            working = false;
        }

        private static void UniversalBarrier(ILContext il)
        {
            ILCursor ic = new(il);
            while (ic.TryGotoNext(MoveType.After, x => { x.MatchCall(out var a); return a?.Name == "get_Current"; },
                x => { return x.MatchCastclass<Actor>(); }))
            {
                ic.EmitDelegate(nameless1);
                static Actor nameless1(Actor a)
                {
                    var tar = Engine.Scene.Tracker.Entities[typeof(CustomInvisibleBarrier)].Cast<CustomInvisibleBarrier>();
                    foreach (var r in tar)
                    {
                        r.ApplyTo(a);
                    }
                    return a;
                }
            }
            ic = new ILCursor(il);
            while (ic.TryGotoNext(MoveType.AfterLabel, x => x.MatchRet()))
            {
                ic.EmitLdarg0();
                ic.EmitDelegate(nameless2);
                static void nameless2(Entity self)
                {
                    var tar = Engine.Scene.Tracker.Entities[typeof(CustomInvisibleBarrier)].Cast<CustomInvisibleBarrier>();
                    foreach (var r in tar)
                    {
                        r.ApplyTo(self);
                    }
                }
                ic.Index++;
            }
        }

        public CustomInvisibleBarrierManager(EntityData e, Vector2 offset)
            : this(e.Bool("actorsolid", true), e.Bool("jumpthru", true), e.Bool("moddedjumpthru", true), e.Bool("jelly", false))
        {

        }
        internal static void EnsureManager(Scene scene)
        {
            var has = scene.Tracker.GetEntity<CustomInvisibleBarrierManager>();
            if (has is null)
            {
                scene.Add(has = new CustomInvisibleBarrierManager());
            }
        }
    }
    [CustomEntity("ReverseHelper/CustomInvisibleBarrier")]
    [Tracked(false)]
    [SourceGen.Loader.LazyLoad]
    public class CustomInvisibleBarrier : Solid
    {
        public HashSet<Type>? typer;
        List<string>? resttype;
        EntityData ed;
        bool awaken = false;
        public bool reversed = false;
        bool killins_unused;
        public void ApplyTo(Entity type)
        {
            var match = typer!.Contains(type.GetType());
            Collidable = reversed != match;
            if (ThruMode != JumpThruMode.none && Collidable)
            {
                Rectangle result = OrigSize;
                switch (ThruMode)
                {
                    case JumpThruMode.up:
                    case JumpThruMode.down:
                        if (type.Right <= Left || type.Left >= Right)
                        {
                            Collidable = false;
                            return;
                        }
                        break;
                    case JumpThruMode.left:
                    case JumpThruMode.right:
                        if (type.Bottom <= Top || type.Top >= Bottom)
                        {
                            Collidable = false;
                            return;
                        }
                        break;
                }
                switch (ThruMode)
                {
                    case JumpThruMode.up:
                        result.Y = (int)type.Bottom;
                        break;
                    case JumpThruMode.down:
                        result.Y = (int)type.Top - result.Height;
                        break;
                    case JumpThruMode.left:
                        result.X = (int)type.Right;
                        break;
                    case JumpThruMode.right:
                        result.X = (int)type.Left - result.Width;
                        break;
                }
                var t = OrigSize;
                t = t.ClampTo(result);
                if (t.Width <= 0 || t.Height <= 0)
                {
                    Collidable = false;
                    return;
                }
                Position.X = t.X;
                Position.Y = t.Y;
                Collider.Width = t.Width;
                Collider.Height = t.Height;
            }
        }
        public void Restore()
        {
            Collidable = reversed;
        }
        public enum JumpThruMode
        {
            none, left, right, up, down,
        }
        JumpThruMode ThruMode;
        Rectangle OrigSize;
        public CustomInvisibleBarrier(Vector2 position, int width, int height, EntityData e, bool rev, bool climb, bool kill, bool attach, JumpThruMode js) : base(position, width, height, true)
        {
            if (e.Values.TryGetValue("_resttype", out var a))
            {
                resttype = (List<string>?)a;
            }
            if (e.Values.TryGetValue("_pre_parse", out var b))
            {
                typer = (HashSet<Type>?)b;
            }
            Collidable = false;
            ed = e;
            reversed = rev;
            if (!climb)
            {
                Add(new ClimbBlocker(true));
            }
            killins_unused = kill;
            if (attach)
            {
                Add(new StaticMover());
            }
            OrigSize = new((int)position.X, (int)position.Y, width, height);

            ThruMode = js;
            if (ThruMode != JumpThruMode.none && reversed)
            {
                throw new InvalidOperationException("CustomInvisibleBarrier JumpThru Mode can't be reversed, or you will comes to the performance hell.");
            }
        }

        public CustomInvisibleBarrier(EntityData e, Vector2 offset) : this(e.Position + offset, e.Width, e.Height, e, e.Bool("reversed"), e.Bool("climb"), e.Bool("killInside"), e.Bool("attach"), e.Enum("jumpThruMode", JumpThruMode.none))
        {

        }
        [SourceGen.Loader.Load]
        public static void Load()
        {
            //On.Celeste.LevelLoader.ctor += LevelLoader_ctor; ;
            On.Celeste.OverworldLoader.ctor += OverworldLoader_ctor;
            //Everest.Events.Everest.OnLoadMod += Everest_OnRegisterModule;
        }

        private static void Everest_OnRegisterModule(EverestModule module)
        {
            resolved_type.Clear();
            type_resolved.Clear();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            CustomInvisibleBarrierManager.EnsureManager(scene);
        }

        static IEnumerable<(string s, Type t)> map(Type t)
        {
            if (type_resolved.Contains(t))
            {
                yield break;
            }
            type_resolved.Add(t);
            yield return (t.Name, t);
            yield return (t.FullName!, t);
            foreach (var s in t.GetCustomAttribute<CustomEntityAttribute>()?.IDs ?? [])
            {
                yield return (s.Split('=')[0].Trim(), t);
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (awaken)
            {
                return;
            }
            foreach (var (k, v) in SceneAs<Level>().Entities.SelectMany(x => map(x.GetType())))
            {
                resolved_type.TryAdd(k, v);
            }
            //.Concat(resolved_type.Select(x=>(x.Key,x.Value))).ToDictionary(x=>x.Item1,x=>x.Item2);

            try
            {
                foreach (var r in Scene.Tracker.GetEntities<CustomInvisibleBarrier>())
                {
                    (r as CustomInvisibleBarrier)?.TryAwake();
                }
            }
            catch (Exception)
            {
            }

        }
        public void TryAwake()
        {
            if (awaken)
            {
                return;
            }
            if (typer is null)
            {
                string s = ed.Attr("type");
                ed.Values["type"] = "";
                ed.Values.Add("_pre_parse", typer = new HashSet<Type>());
                ed.Values.Add("_resttype", resttype = new List<string>(s.Split(',').Select(x => x.Trim())));
            }
            var r_resttype = new List<string>();
            foreach (var v in resttype!)
            {
                if (resolved_type.TryGetValue(v, out var type))
                {
                    typer.Add(type);
                }
                else
                {
                    r_resttype.Add(v);
                }
            }
            if (r_resttype.Any())
            {
                var rr_rest = new List<string>();
                foreach (var v in r_resttype)
                {
                    bool success = false;
                    if (!v.Contains('.'))
                    {
                        try
                        {
                            if (Level.EntityLoaders.TryGetValue(v, out var loader))
                            {
                                var tar = loader.Target;
                                Type? tars = null;
                                foreach (var k in tar!.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                                {
                                    switch (k.GetValue(loader))
                                    {
                                        case ConstructorInfo { } ci:
                                            tars = ci.DeclaringType;
                                            break;
                                        case MethodInfo { } mi:
                                            var tmp = mi.DeclaringType;
                                            if (tmp!.IsSubclassOf(typeof(Entity)))
                                            {
                                                tars = tmp;
                                            }
                                            else
                                            {
                                                //DynamicMethodDefinition dmd = new(mi);
                                                //ILContext il = new(dmd.Definition);

                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                if (tars is not null)
                                {
                                    resolved_type[v] = tars;
                                    success = true;
                                    typer.Add(tars);
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                    if (!success)
                    {
                        rr_rest.Add(v);
                    }
                }
                if (rr_rest.Any())
                {
                    ed.Values["_resttype"] = resttype = rr_rest;
                    Logger.Log(LogLevel.Warn, "ReverseHelper", $"Failed when loading {string.Join(",", rr_rest)} for {nameof(CustomInvisibleBarrier)}, they does not exists in current room. (at least now.) Please add one from map editor so it could be found successfully.");
                }
            }
            foreach (var v in typer)
            {
                var vx = v;
                while (vx.BaseType != null)
                {
                    if (vx == typeof(Actor))
                    {
                        CustomInvisibleBarrierManager.EnsureManager(Scene);
                        break;
                    }
                    vx = vx.BaseType;
                }
                EnsureHooks(v);
            }
            awaken = true;
        }
        static Dictionary<string, Type> resolved_type = [];
        static HashSet<Type> type_resolved = [];
        //private static bool Level_OnLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        //{
        //	if (entityData.Name == "ReverseHelper/CustomInvisibleBarrier")
        //	{
        //		var t = entityData.Attr("");
        //		entityData.Values["type"] = "";
        //		List<string> unre;

        //		foreach (var v in unre)
        //		{
        //			if (resolved_type.TryGetValue(v, out var type))
        //			{
        //				types.Add(type);
        //			}
        //		}
        //		foreach (var v in t.Split(',').Select(x => x.Trim()))
        //		{
        //			if (resolved_type.TryGetValue(v,out var type))
        //			{
        //				types.Add(type);
        //			}
        //			else
        //			{
        //				unre.Add(v);
        //			}
        //		}
        //	}
        //	return false;
        //}

        private static void OverworldLoader_ctor(On.Celeste.OverworldLoader.orig_ctor orig, OverworldLoader self, Overworld.StartMode startMode, HiresSnow snow)
        {
            orig(self, startMode, snow);
            RemoveAllHooks();
        }
        public static void EnsureHooks(Type type)
        {
            if (Hooks.ContainsKey(type))
            {
                return;
            }
            MethodInfo? update = type.GetMethod("Update", BindingFlags.Instance | BindingFlags.Public);
            //while (update is null)
            //{
            //    type = type.BaseType;
            //    if (type is null || type.FullName == "Monocle.Entity") 
            //    {
            //        return;
            //    }
            //    update = type.GetMethod("Update", BindingFlags.Instance);
            //}
            if (update is null)
            {
                Logger.Log(LogLevel.Error, "ReverseHelper", $"Failed when loading {type.FullName} for {nameof(CustomInvisibleBarrier)}. It has no Method named Update(). You could try the entity it derives from.");
                return;
            }
            Hook h = new(update, (Action<Entity> orig, Entity self) =>
            {
                var tar = Engine.Scene.Tracker.Entities[typeof(CustomInvisibleBarrier)].Cast<CustomInvisibleBarrier>();
                foreach (var r in tar)
                {
                    r.ApplyTo(self);
                }

                orig(self);
                foreach (var r in tar)
                {
                    r.Restore();
                }
            });
            Hooks.Add(type, h);

        }
        static Dictionary<Type, Hook> Hooks = new();
        public static void RemoveAllHooks()
        {
            foreach (var hook in Hooks)
            {
                hook.Value.Dispose();
            }
            Hooks.Clear();
        }
        [SourceGen.Loader.Unload]
        public static void Unload()
        {
            //On.Celeste.LevelLoader.ctor-= LevelLoader_ctor;
            On.Celeste.OverworldLoader.ctor -= OverworldLoader_ctor;
            RemoveAllHooks();

            //Everest.Events.Level.OnLoadEntity -= Level_OnLoadEntity;
        }
        public override void Update()
        {
            base.Update();
            //foreach(var en in Scene.Entities)
            //{
            //    try
            //    {
            //    if(Collider.Collide(en)&&en is Player act&&typer.Contains(en.GetType())!=reversed)
            //    {
            //        act.Die(Vector2.Zero);
            //    }

            //    }
            //    catch (Exception)
            //    {

            //    }
            //}
        }
    }
}