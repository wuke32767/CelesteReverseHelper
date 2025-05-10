using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [CustomEntity("ReverseHelper/DashAttackRefill")]
    public class DashAttackRefill : Refill
    {
        public DashAttackRefill(Vector2 pos, bool oneuse, float dashAttack, float respawn, string text) : base(pos, false, oneuse)
        {
            Get<PlayerCollider>().OnCollide = p =>
            {
                p.dashAttackTimer = dashAttack;
                Audio.Play("event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;
                Add(new Coroutine(RefillRoutine(p)));
                respawnTimer = respawn;
            };
            sprite.Reset(GFX.Game, text + "idle");
            outline.Texture = GFX.Game[text + "outline"];
            flash.Reset(GFX.Game, text + "flash");
        }

        public DashAttackRefill(EntityData e, Vector2 offset)
            : this(e.Position + offset, e.Bool("oneUse"), e.Float("dashAttackTime"),
                  e.Float("respawnTime", 2.5f), e.Attr("image", "objects/refill/"))
        {
        }
    }
}