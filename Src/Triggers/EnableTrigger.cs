using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

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
    public record struct EnableTriggerSessionData(bool triggered);
    [CustomEntity("ReverseHelper/EnableTrigger")]
    //[Tracked]
    public class EnableTrigger(EntityData e, Vector2 offset) : Trigger(e, offset)
    {
        public class Session()
        {
            public Dictionary<int, EnableTriggerSessionData> Triggered { get; set; } = [];
        }
        enum Mode
        {
            MoveAway, Disable, Shrink, Collidable,
        }
        Vector2[] Nodes = e.Nodes?.Select(x => x + offset).ToArray() ?? [];
        bool reversed = e.Bool("reversed");
        bool persistent = e.Bool("persistent");
        Mode mode = e.Enum("mode", Mode.Collidable);
        bool revert = e.Bool("revertOnExit");
        bool oneuse = e.Bool("oneUse");
        bool toggleswitch = e.Bool("toggleSwitchMode");
        bool lazy = e.Bool("lazy");
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

            if (persistent)
            {
                if (ReverseHelperModule.Session.EnableTrigger.Triggered.TryGetValue(e.ID, out var val)
                    && val.triggered)
                {
                    OnEnd();
                }
            }


            run(false);
        }
        Trigger[] target = default!;
        Vector2[] SavDat = default!;
        Vector2 Tar = default!;
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            OnStart();
            if (persistent)
            {
                var dic = ReverseHelperModule.Session.EnableTrigger.Triggered;
                var re = dic.GetValueOrDefault(e.ID);
                dic[e.ID] = new(!re.triggered);
            }
        }

        private void OnStart()
        {
            run(true);
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            OnEnd();
        }

        private void OnEnd()
        {
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