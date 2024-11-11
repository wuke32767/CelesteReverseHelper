using System.Runtime.CompilerServices;

namespace Celeste.Mod.ReverseHelper.Libraries
{
    enum TrackMode
    {
        Full, /*CommunalHelperDreamTunnel*/CHDT, RevProxy, Jank,
    }
    interface IDepthTracker
    {
        public Action Renderer { get; set; }
        public static IDepthTracker Track(Entity trackto, Entity self, TrackMode tm)
        {
            return tm switch
            {
                TrackMode.Full => FullDepthTracker.Track(trackto, self),
                TrackMode.CHDT => CHDTDepthTracker.Track(trackto, self),
                TrackMode.RevProxy => ProxyDepthTracker.Track(trackto, self),
                //TrackMode.Jank => JankDepthTracker.Track(trackto, self),
                _ => throw new NotImplementedException(),
            };
        }
        public void Untrack(Entity self);
    }

    internal class ProxyDepthTracker : CHDTDepthTracker, IDepthTracker
    {
        //static ConditionalWeakTable<Entity, ProxyDepthTracker> tracker = [];
        public static new ProxyDepthTracker Track(Entity trackto, Entity self)
        {
            var i = new ProxyDepthTracker(self, trackto);
            self.Scene?.Add(i);
            return i;
        }
        class Helper : Entity
        {
            public required Action Upd;
            public override void Render()
            {
                base.Render();
                Upd();
            }
        }
        Helper Dummy;
        private readonly Entity solid;

        bool f = false;
        public ProxyDepthTracker(Entity db, Entity Solid) : base(db, Solid)
        {
            solid = Solid;
            Dummy = new()
            {
                Upd = () =>
                {
                    f = Solid.Visible;
                    Solid.Visible = false;
                }
            };
        }

        public override void Render()
        {
            base.Render();
            solid.Visible = f;

            if (solid.Visible)
            {
                solid.Render();
            }
            RRender();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }
        protected override void UPD()
        {
            base.UPD();
            Dummy.Depth = solid.Depth + 1;

        }
    }

    class CHDTDepthTracker(Entity db, Entity Solid) : Entity(), IDepthTracker
    {
        protected Action RRender;
        Action IDepthTracker.Renderer { get => () => { }; set => RRender = value; }
        public override void Added(Scene scene)
        {
            base.Added(scene);

            UPD();
        }

        protected virtual void UPD()
        {
            Depth = Solid.Depth;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }

        public override void Update()
        {
            base.Update();
            //if (db.Scene is null)
            //{
            //    RemoveSelf();
            //}
            //else
            {
                UPD();
            }
        }

        public override void Render()
        {
            base.Render();
            if (db.Scene is not null && db.Visible)
            {
                RRender();
            }
        }

        internal static CHDTDepthTracker Track(Entity trackto, Entity self)
        {
            var i = new CHDTDepthTracker(self, trackto);
            self.Scene?.Add(i);
            return i;
        }
        void IDepthTracker.Untrack(Entity self)
        {
            RemoveSelf();
        }
    }
    internal sealed class JankDepthTracker : IDepthTracker
    {
        public Entity Entity;
        public Scene? Scene => Entity.Scene;

        public Action Renderer { get; set; }


        public List<Entity> target = [];
        public static JankDepthTracker Track(Entity trackto, Entity self)
        {
            var tracker = Tracker.GetOrCreateValue(trackto);
            tracker.target.Add(self);
            if (OnScene<JankDepthTracker>.Construct(self.Scene))
            {
                OnScene<JankDepthTracker>.self.Add(new PostUpdateHook(() =>
                {

                }));
            }

            tracker.Entity = trackto;
            trackto.PreUpdate += tracker.Apply;
            return tracker;
        }

        public JankDepthTracker()
        {
        }

        class some_cmp : IComparer<Entity>
        {
            public int Compare(Entity? a, Entity? b) => Math.Sign(b!.actualDepth - a!.actualDepth);
            public static readonly some_cmp Instance = new();
        }

        public void Apply(Entity ex)
        {
            if (Scene is not null && !Scene.Entities.unsorted)
            {
                var entities = Scene.Entities.entities;
                var i = entities.BinarySearch(Entity, some_cmp.Instance);
                if (i >= 0 && entities[i] == Entity)
                {
                    if (entities.Count >= i + 2 && target[0] != entities[i + 1])
                    {
                        int d = Entity.Depth;
                        var cur = Entity.actualDepth;
                        foreach (var single in target.AsEnumerable().Reverse())
                        {
                            cur = single.actualDepth = double.BitDecrement(cur);
#pragma warning disable CS0618 // intended
                            single.depth = d;
#pragma warning restore CS0618 
                        }

                        for (int j = i + 1; j < entities.Count && entities[j].Depth == d; j++)
                        {
                            if (cur <= entities[j].actualDepth)
                            {
                                cur = entities[j].actualDepth = double.BitDecrement(cur);
                            }
                            else
                            {
                                break;
                            }
                        }

                        Scene.actualDepthLookup[d] = double.Max(Scene.actualDepthLookup[d], (d - cur) + 1e-6f);
                        Scene.Entities.MarkUnsorted();
                    }
                    ex.PreUpdate -= Apply;
                    return;
                }
            }
        }

        private static ConditionalWeakTable<Entity, JankDepthTracker> Tracker = [];

        public void Untrack(Entity entity)
        {
            var at = target.IndexOf(entity);
            target[at] = target[^1];
            target.RemoveAt(target.Count - 1);
            if (target.Count == 0)
            {
                Tracker.Remove(Entity);
            }
        }
    }
    internal sealed class FullDepthTracker : IDepthTracker
    {
        public Entity Entity;
        public Scene? Scene => Entity.Scene;

        public Action Renderer { get; set; }


        public List<Entity> target = [];

        public static FullDepthTracker Track(Entity trackto, Entity self)
        {
            var tracker = Tracker.GetOrCreateValue(trackto);
            tracker.target.Add(self);
            tracker.Entity = trackto;
            trackto.PreUpdate += tracker.Apply;
            return tracker;
        }

        public FullDepthTracker()
        {
        }

        class some_cmp : IComparer<Entity>
        {
            public int Compare(Entity? a, Entity? b) => Math.Sign(b!.actualDepth - a!.actualDepth);
            public static readonly some_cmp Instance = new();
        }

        public void Apply(Entity ex)
        {
            if (Scene is not null && !Scene.Entities.unsorted)
            {
                var entities = Scene.Entities.entities;
                var i = entities.BinarySearch(Entity, some_cmp.Instance);
                if (i >= 0 && entities[i] == Entity)
                {
                    if (entities.Count >= i + 2 && target[0] != entities[i + 1])
                    {
                        int d = Entity.Depth;
                        var cur = Entity.actualDepth;
                        foreach (var single in target.AsEnumerable().Reverse())
                        {
                            cur = single.actualDepth = double.BitDecrement(cur);
#pragma warning disable CS0618 // intended
                            single.depth = d;
#pragma warning restore CS0618 
                        }

                        for (int j = i + 1; j < entities.Count && entities[j].Depth == d; j++)
                        {
                            if (cur <= entities[j].actualDepth)
                            {
                                cur = entities[j].actualDepth = double.BitDecrement(cur);
                            }
                            else
                            {
                                break;
                            }
                        }

                        Scene.actualDepthLookup[d] = double.Max(Scene.actualDepthLookup[d], (d - cur) + 1e-6f);
                        Scene.Entities.entities.Sort(EntityList.CompareDepth);
                    }
                    ex.PreUpdate -= Apply;
                    return;
                }
            }
        }

        //[SourceGen.Loader.Load]
        //[SourceGen.Loader.LazyLoad]
        public static void Load()
        {
            On.Monocle.Scene.SetActualDepth += Scene_SetActualDepth;
        }

        // according to benchmark, cwt is faster than dic
        // but why
        private static ConditionalWeakTable<Entity, FullDepthTracker> Tracker = [];

        private static void Scene_SetActualDepth(On.Monocle.Scene.orig_SetActualDepth orig, Scene self, Entity entity)
        {
            orig(self, entity);
            if (Tracker.TryGetValue(entity, out var dt))
            {
                foreach (var e2 in dt.target)
                {
                    set_Depth(e2, entity.Depth);
                }
            }

#pragma warning disable CS0618 // intended
            static void set_Depth(Entity self, int value)
            {
                if (self.depth != value)
                {
                    self.depth = value;
                    if (self.Scene != null)
                    {
                        SetActualDepth(self.Scene, self);
                    }
                }
            }

            static void SetActualDepth(Scene scene, Entity entity)
            {
                double value = 0.0;
                if (scene.actualDepthLookup.TryGetValue(entity.depth, out value))
                {
                    scene.actualDepthLookup[entity.depth] += 1e-6f;
                }
                else
                {
                    scene.actualDepthLookup.Add(entity.depth, 1e-6f);
                }

                entity.actualDepth = entity.depth - value;
                scene.Entities.MarkUnsorted();
            }
#pragma warning restore CS0618
        }

        //[SourceGen.Loader.Unload]
        public static void Unload()
        {
            On.Monocle.Scene.SetActualDepth -= Scene_SetActualDepth;
        }

        public void Untrack(Entity entity)
        {
            var at = target.IndexOf(entity);
            target[at] = target[^1];
            target.RemoveAt(target.Count - 1);
            if (target.Count == 0)
            {
                Tracker.Remove(Entity);
            }
        }
    }
}