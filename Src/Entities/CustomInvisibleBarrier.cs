﻿using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [Tracked]
    [SourceGen.Loader.Preload("ReverseHelper/BarrierInteropHelper")]
    public partial class CustomInvisibleBarrierManager : Entity
    {
        bool actorsolid;
        bool jump;
        bool mod;

        public CustomInvisibleBarrierManager(bool actorsolid=true, bool jump=true, bool mod=true) : base()
        {
            this.actorsolid = actorsolid;
            this.jump = jump;
            this.mod = mod;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if(actorsolid)
            {

            }
            if(jump)
            {

            }
            if(mod)
            {

            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if(actorsolid)
            {

            }
            if(jump)
            {

            }
            if(mod)
            {

            }
        }

        public CustomInvisibleBarrierManager(EntityData e, Vector2 offset)
            : this(e.Bool("actorsolid", true), e.Bool("jumpthru", true), e.Bool("moddedjumpthru", true))
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
    public class CustomInvisibleBarrier : Solid
    {
        HashSet<Type>? typer;
        List<string>? resttype;
        EntityData ed;
        bool awaken = false;
        bool reversed = false;
        static bool has_actor;
        bool this_has_actor = false;
        bool killins_unused;

        public CustomInvisibleBarrier(Vector2 position, int width, int height, EntityData e, bool rev, bool climb, bool kill, bool attach) : base(position, width, height, true)
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
        }

        public CustomInvisibleBarrier(EntityData e, Vector2 offset) : this(e.Position + offset, e.Width, e.Height, e, e.Bool("reversed"), e.Bool("climb"), e.Bool("killInside"), e.Bool("attach"))
        {

        }
        [SourceGen.Loader.Load]
        [SourceGen.Loader.LazyLoad]
        public static void Load()
        {
            //On.Celeste.LevelLoader.ctor += LevelLoader_ctor; ;
            On.Celeste.OverworldLoader.ctor += OverworldLoader_ctor;
            IL.Celeste.Solid.MoveHExact += UniversalBarrier;
            IL.Celeste.Solid.MoveVExact += UniversalBarrier;
            IL.Celeste.JumpThru.MoveHExact += UniversalBarrier;
            IL.Celeste.JumpThru.MoveVExact += UniversalBarrier;
        }
        static ILHook? AttachedJumpThru;
        //workaround here
        public static void LoadContent()
        {
            var mov = ReverseHelperExtern.VortexHelperModule.AttachedJumpThru.MoveHExact;
            if (mov is not null && AttachedJumpThru is null)
            {
                AttachedJumpThru = new ILHook(mov, UniversalBarrier);
            }
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
                    if (!has_actor)
                    {
                        return a;
                    }
                    var tar = Engine.Scene.Tracker.Entities[typeof(CustomInvisibleBarrier)].Cast<CustomInvisibleBarrier>();
                    foreach (var r in tar)
                    {
                        r.Collidable = r.reversed != r.typer!.Contains(a.GetType());
                    }
                    return a;
                }
            }
            ic = new ILCursor(il);
            while (ic.TryGotoNext(MoveType.Before, x => x.MatchRet()))
            {
                ic.EmitDelegate(nameless2);
                static void nameless2()
                {
                    if (!has_actor)
                    {
                        return;
                    }
                    var tar = Engine.Scene.Tracker.Entities[typeof(CustomInvisibleBarrier)].Cast<CustomInvisibleBarrier>();
                    foreach (var r in tar)
                    {
                        r.Collidable = r.reversed;
                    }
                }
                ic.Index++;
            }
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
            foreach (var s in (Attribute.GetCustomAttribute(t, typeof(CustomEntityAttribute)) as CustomEntityAttribute)?.IDs ?? Enumerable.Empty<string>())
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
            has_actor = false;
            foreach (var (k, v) in SceneAs<Level>().Entities.SelectMany(x => map(x.GetType())))
            {
                try
                {
                    resolved_type.Add(k, v);
                }
                catch
                {
                }
            }
            //.Concat(resolved_type.Select(x=>(x.Key,x.Value))).ToDictionary(x=>x.Item1,x=>x.Item2);

            try
            {
                foreach (var r in Scene.Tracker.Entities[typeof(CustomInvisibleBarrier)])
                {
                    (r as CustomInvisibleBarrier)?.TryAwake();
                }
            }
            catch (Exception)
            {
            }

            if (!has_actor)
            {
                has_actor = Scene.Tracker.Entities[typeof(CustomInvisibleBarrier)].Cast<CustomInvisibleBarrier>().Any(x => x.this_has_actor);
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
                ed.Values["_resttype"] = r_resttype;
                //foreach (var v in r_resttype)
                {
                    Logger.Log(LogLevel.Warn, "ReverseHelper", $"Failed when loading {string.Join(",",r_resttype)} for {nameof(CustomInvisibleBarrier)}, they does not exists in current room. (at least now.) Please add one from map editor so it could be found successfully.");
                }
            }
            foreach (var v in typer)
            {
                var vx = v;
                while (vx.BaseType != null)
                {
                    if (vx == typeof(Actor))
                    {
                        this_has_actor = true;
                        has_actor = true;
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
                var tar = Engine.Scene.Tracker.Entities[typeof(CustomInvisibleBarrier)].Cast<CustomInvisibleBarrier>().Where(x => x.typer!.Contains(type));
                foreach (var r in tar)
                {
                    r.Collidable = !r.reversed;
                }

                orig(self);
                foreach (var r in tar)
                {
                    r.Collidable = r.reversed;
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
            IL.Celeste.Solid.MoveHExact -= UniversalBarrier;
            IL.Celeste.Solid.MoveVExact -= UniversalBarrier;
            IL.Celeste.JumpThru.MoveHExact -= UniversalBarrier;
            IL.Celeste.JumpThru.MoveVExact -= UniversalBarrier;

            AttachedJumpThru?.Dispose();
            AttachedJumpThru = null;
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