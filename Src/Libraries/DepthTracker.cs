using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core.Tokens;

namespace Celeste.Mod.ReverseHelper.Libraries
{
    internal class DepthTracker : Component
    {
        public Entity target;
        //if target.actualDepth is not set.
        //if it's not dirty, no need to update.
        bool dirty { get => Active; set => Active = value; }
        public static DepthTracker Track(Entity trackto, Entity self)
        {
            var tracker = new DepthTracker(self);
            trackto.Add(tracker);
            tracker.Apply();
            return tracker;
        }
        public DepthTracker(Entity self) : base(true, false)
        {
            target = self;
        }
        class some_cmp : IComparer<Entity>
        {
            public int Compare(Entity? a, Entity? b) => Math.Sign(b!.actualDepth - a!.actualDepth);
            public static some_cmp Instance = new();
        }
        public void Apply()
        {
            if (!Scene.Entities.unsorted)
            {
                var i = Scene.Entities.entities.BinarySearch(Entity, some_cmp.Instance);
                if (i >= 0 && Scene.Entities.entities[i] == Entity)
                {
                    int d = Entity.Depth;
                    Scene.actualDepthLookup[d] += 9.9999999747524271E-07;
                    foreach (var e in Scene.Entities.entities.Skip(i + 1).TakeWhile(e => e.Depth == d))
                    {
                        e.actualDepth -= 9.9999999747524271E-07;
                    }
                    target.actualDepth = Entity.actualDepth - 9.9999999747524271E-07;
                    target.depth = d;
                    Scene.Entities.MarkUnsorted();
                    dirty = false;
                    return;
                }
            }
        }
        public override void Update()
        {
            base.Update();
            //if (dirty)
            {
                Apply();
            }
        }
        [SourceGen.Loader.Load]
        [SourceGen.Loader.LazyLoad]
        public static void Load()
        {
            On.Monocle.Scene.SetActualDepth += Scene_SetActualDepth;
        }

        private static void Scene_SetActualDepth(On.Monocle.Scene.orig_SetActualDepth orig, Scene self, Entity entity)
        {
            orig(self, entity);
            foreach (var dt in entity.Components.OfType<DepthTracker>())
            {
                set_Depth(dt.target, entity.Depth);
                //if (dt.target.Scene is null)
                //{
                //    dt.RemoveSelf();
                //}
                dt.dirty = false;
            }
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
                    scene.actualDepthLookup[entity.depth] += 9.9999999747524271E-07;
                }
                else
                {
                    scene.actualDepthLookup.Add(entity.depth, 9.9999999747524271E-07);
                }

                entity.actualDepth = entity.depth - value;
                scene.Entities.MarkUnsorted();
                //for (int i = 0; i < BitTag.TotalTags; i++)
                //{
                //    if (entity.TagCheck(1 << i))
                //    {
                //        scene.TagLists.MarkUnsorted(i);
                //    }
                //}
            }
        }
        [SourceGen.Loader.Unload]
        public static void Unload()
        {
            On.Monocle.Scene.SetActualDepth -= Scene_SetActualDepth;
        }
    }
}
