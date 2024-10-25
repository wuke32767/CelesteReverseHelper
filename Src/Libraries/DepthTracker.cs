using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Celeste.Mod.ReverseHelper.Entities;
using YamlDotNet.Core.Tokens;

namespace Celeste.Mod.ReverseHelper.Libraries
{
    internal class DepthTracker
    {
        public Entity Entity;
        public Scene? Scene => Entity.Scene;
        public List<Entity> target = [];

        public static DepthTracker Track(Entity trackto, Entity self)
        {
            var tracker = Tracker.GetOrCreateValue(trackto);
            tracker.target.Add(self);
            tracker.Entity = trackto;
            trackto.PreUpdate += tracker.Apply;
            return tracker;
        }

        public DepthTracker()
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
                    if (entities.Count >= i + 2 && target[0]!=entities[i + 1])
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

        //[SourceGen.Loader.Load]
        //[SourceGen.Loader.LazyLoad]
        public static void Load()
        {
            On.Monocle.Scene.SetActualDepth += Scene_SetActualDepth;
        }

        // according to benchmark, cwt is faster than dic
        // but why
        private static ConditionalWeakTable<Entity, DepthTracker> Tracker = [];

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

#pragma warning disable CS0618 // depth is expected.
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