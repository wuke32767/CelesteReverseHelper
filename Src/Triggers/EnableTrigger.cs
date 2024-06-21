using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.ReverseHelper.Triggers
{

    [CustomEntity("ReverseHelper/EnableTriggerTarget")]
    [Tracked]
    public class EnableTriggerTarget(EntityData e, Vector2 offset) : Entity(e.Position + offset)
    {
        public override void Added(Scene scene)
        {
            Collider = new Hitbox(e.Width, e.Height);
            base.Added(scene);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            RemoveSelf();
        }
    }
    [CustomEntity("ReverseHelper/EnableTrigger")]
    public class EnableTrigger(EntityData e, Vector2 offset) : Trigger(e, offset)
    {
        enum Mode
        {
            MoveAway, Disable, Shrink, Collidable,
        }
        Vector2[] Nodes = e.Nodes?.Select(x => x + offset).ToArray() ?? [];
        bool reversed = e.Bool("reversed");
        Mode mode = e.Enum("mode", Mode.Collidable);
        bool revert = e.Bool("revertOnExit");
        bool oneuse = e.Bool("oneUse");
        bool toggleswitch = e.Bool("toggleSwitchMode");
        void run(bool b)
        {
            if (mode == Mode.Disable)
            {
                foreach (var node in target)
                {
                    node.Active = reversed != b;
                }
            }
            else if (mode == Mode.Collidable)
            {
                foreach (var node in target)
                {
                    node.Collidable = reversed != b;
                }
            }
            else if (mode == Mode.MoveAway)
            {
                if (reversed != b)
                {
                    foreach (var (node, orig) in target.Zip(SavDat))
                    {
                        node.Position = orig;
                    }
                }
                else
                {
                    foreach (var node in target)
                    {
                        node.Position = Tar;
                    }
                }
            }
            else if (mode == Mode.Shrink)
            {
                if (reversed != b)
                {
                    foreach (var (node, orig) in target.Zip(SavDat))
                    {
                        node.Collider.Width = orig.X;
                        node.Collider.Height = orig.Y;
                    }
                }
                else
                {
                    foreach (var node in target)
                    {
                        node.Collider.Width = 0;
                        node.Collider.Height = 0;
                    }
                }
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            var r = from node in Nodes
                    from child in Scene.CollideAll<Trigger>(node)
                    where child != this
                    select child;
            target = r.ToArray();
            if (mode == Mode.MoveAway)
            {
                Tar = Nodes.Select(Scene.CollideFirst<EnableTriggerTarget>)
                    .FirstOrDefault(x => x is not null)?.Position ?? Position;
                SavDat = target.Select(x => x.Position).ToArray();
            }
            else if (mode == Mode.Shrink)
            {
                SavDat = target.Select(x => x.Collider.Size).ToArray();
            }
            run(false);
        }
        Trigger[] target = default!;
        Vector2[] SavDat = default!;
        Vector2 Tar = default!;
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            run(true);
        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (revert)
            {
                run(false);
            }
            if (oneuse)
            {
                RemoveSelf();
            }
            if (toggleswitch)
            {
                reversed = !reversed;
            }
        }
    }
}