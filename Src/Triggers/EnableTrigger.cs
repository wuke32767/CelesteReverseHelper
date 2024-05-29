using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Linq;

namespace Celeste.Mod.ReverseHelper.Triggers
{
    [CustomEntity("ReverseHelper/EnableTrigger")]
    public class EnableTrigger(EntityData e, Vector2 offset) : Trigger(e, offset)
    {
        Vector2[] Nodes = e.Nodes?.Select(x => x + offset).ToArray() ?? [];
        bool reversed = e.Bool("reversed");
        bool revert = e.Bool("revertOnExit");
        bool oneuse = e.Bool("oneUse");
        bool toggleswitch = e.Bool("toggleSwitchMode");
        void run(bool b)
        {
            foreach (var node in target)
            {
                node.Collidable = reversed != b;
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
            run(false);
        }
        Trigger[] target = default!;
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
            if(toggleswitch)
            {
                reversed = !reversed;
            }
        }
    }
}