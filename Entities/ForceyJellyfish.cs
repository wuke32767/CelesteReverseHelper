using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Reflection;

namespace Celeste.Mod.ReverseHelper.Entities
{
    // Token: 0x020001AC RID: 428
    [CustomEntity("ReverseHelper/ForceyJellyfish")]
    public class ForceyJellyfish : Glider
    {
        // Token: 0x06000EF2 RID: 3826 RVA: 0x00039F34 File Offset: 0x00038134
        public ForceyJellyfish(Vector2 position, bool bubble, bool tutorial, float force) : base(position, bubble, tutorial)
        {
            pushForce = force;
            Hold.OnRelease = new Action<Vector2>(this.OnRelease);
            Hold.OnPickup = new Action(this.OnPickup);
        }

        // Token: 0x06000EF3 RID: 3827 RVA: 0x0003A0EC File Offset: 0x000382EC
        public ForceyJellyfish(EntityData e, Vector2 offset) : this(e.Position + offset, e.Bool("bubble", false), e.Bool("tutorial", false), e.Float("force", 80))
        {
        }

        private static readonly MethodInfo onpic = typeof(Glider).GetMethod("OnPickup", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo onrel = typeof(Glider).GetMethod("OnRelease", BindingFlags.Instance | BindingFlags.NonPublic);

        // Token: 0x06000EFD RID: 3837 RVA: 0x0003AF0C File Offset: 0x0003910C
        private void OnPickup()
        {
            onpic.Invoke(this, null);

            player = Hold.Holder;
        }

        private Player player;

        // Token: 0x06000EFE RID: 3838 RVA: 0x0003AFA4 File Offset: 0x000391A4
        private void OnRelease(Vector2 force)
        {
            onrel.Invoke(this, new Object[] { force });
            if (force.X == 0f)
            {
            }
            else
            {
                if (player == null)
                {
                    return;
                }
                //player.Speed.X += 80f * (float)player.Facing;
                player.Speed.X += pushForce * (float)(-(float)player.Facing);
                player = null;
            }
        }

        private readonly float pushForce;
    }


}