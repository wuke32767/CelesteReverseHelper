using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [CustomEntity("ReverseHelper/EfficientSpinner")]
    [Tracked]
    public class EfficientSpinner : Entity
    {
        static bool initialized = false;

        EfficientPlayerCollider grid;
        int group;
        bool groupLeader;
        public EfficientSpinner(EntityData e, Vector2 offset) : base(e.Position + offset)
        {
            //Add(new HoldableCollider(OnHoldable));
            Add(new LedgeBlocker());
            Depth = -8500;

            //CrystalStaticSpinner
            group = e.Int("group");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!initialized)
            {
                AfterAdded.Reg(scene, () =>
                {
                    var a = scene.Tracker.GetEntities<EfficientSpinner>().OfType<EfficientSpinner>()
                    .GroupBy(x => x.group);
                    foreach (var grp in a)
                    {
                        grp.First().GroupAwake(grp);
                    }
                });
                initialized = true;
            }
        }
        private void GroupAwake(IEnumerable<EfficientSpinner> target)
        {
            CrystalStaticSpinner dummy;
            groupLeader = true;
            var radius = 6;
            var spinner = target.Select(x => x.Position - Position).ToArray();
            var l = spinner.Select(x => x.X).Min() - radius;
            var r = spinner.Select(x => x.X).Max() + radius;
            var t = spinner.Select(x => x.Y).Min() - radius;
            var b = spinner.Select(x => x.Y).Max() + radius;
            var tar = spinner.SelectMany<Vector2, Collider>(x =>
                [new Hitbox(16f, 4f, x.X - 8f, x.Y - 3f), new Circle(radius, x.X, x.Y)]).ToArray();
            Add(grid = new(new(l, t), new(r - l, b - t), p =>
            {
                var m = tar.First(x => x.Collide(p)) switch
                {
                    Hitbox hb => hb.Position + new Vector2(8, 3),
                    Circle c => c.Center,
                    _ => default,
                };
                p.Die((p.Position - m - Position).SafeNormalize());
            }));
            Collider = new ColliderList(tar);
            grid.AddCollider(tar);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!groupLeader)
            {
                RemoveSelf();
            }
        }
    }

}