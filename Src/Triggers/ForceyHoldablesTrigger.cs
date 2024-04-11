using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.ReverseHelper.Triggers
{
    [CustomEntity("ReverseHelper/ForceyHoldablesTrigger")]
    public class ForceyHoldablesTrigger : Trigger
    {
        float force; 
        public ForceyHoldablesTrigger(EntityData e, Vector2 offset) : base(e, offset)
        {
            force= e.Float("force", 120f);
            
        }

        public override void Update()
        {
            base.Update();
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            ForceyHoldablesComponentPlayer.force += force;
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            ForceyHoldablesComponentPlayer.force -= force;
        }
    }
}