using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.ReverseHelper.Entities
{
    // Token: 0x020001AC RID: 428
    [CustomEntity("ReverseHelper/ForceyJellyfish")]
    public class ForceyJellyfish : Glider
    {
        public ForceyJellyfish(Vector2 position, bool bubble, bool tutorial, float force) : base(position, bubble, tutorial)
        {
            Add(new ForceyHoldablesComponent(force));
        }

        public ForceyJellyfish(EntityData e, Vector2 offset) : this(e.Position + offset, e.Bool("bubble", false), e.Bool("tutorial", false), e.Float("force", 80))
        {
        }
    }
}