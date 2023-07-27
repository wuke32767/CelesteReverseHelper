using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;
using Monocle;
using System.Linq;

namespace Celeste.Mod.ReverseHelper.Triggers
{
    [CustomEntity("ReverseHelper/OnlyStrawberryCollectTrigger")]
    public class OnlyStrawberryCollectTrigger : Trigger
    {
        private TypeMatch type;
        private bool delayBetweenBerries;
        private float collectTimer;

        public OnlyStrawberryCollectTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            type = data.Attr("type", "");
            delayBetweenBerries = data.Bool("delayBetweenBerries", false);
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (delayBetweenBerries)
            {
                if (collectTimer < 0)
                {
                    (player.Leader.Followers
                        .FirstOrDefault(x => type.Contains(x.Entity.GetType()))
                        ?.Entity as IStrawberry)
                        ?.OnCollect();
                    collectTimer = 0.3f;
                }
            }
            else
            {
                IStrawberry[] list =
                    player.Leader.Followers
                    .Where(x => type.Contains(x.Entity.GetType()))
                    .Select(x => x.Entity as IStrawberry)
                    .ToArray();

                foreach (var v in list)
                {
                    v?.OnCollect();
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (delayBetweenBerries)
            {
                collectTimer -= Engine.DeltaTime;
            }
        }
    }
}