using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper.Library;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.ReverseHelper.Entities
{
	[CustomEntity("ReverseHelper/ForceyHoldables")]
	internal class ForceyHoldables : Entity
	{
		private TypeMatch wraptype;
		private float pushForce;

		public ForceyHoldables(Vector2 position, float width, float height, string typename, float force) : base(position)
		{

			Collider = new Hitbox(width, height, 0f, 0f);
			wraptype = typename;
			pushForce = force;
		}

		// Token: 0x06000EF3 RID: 3827 RVA: 0x0003A0EC File Offset: 0x000382EC
		public ForceyHoldables(EntityData e, Vector2 offset) : this(e.Position + offset, e.Width, e.Height, e.Attr("type", "Celeste.Grider"), e.Float("force", 80))
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			foreach (Holdable playerCollider in CollideAllByComponent<Holdable>())
			{
				BindEntity(playerCollider.Entity.GetType(), playerCollider.Entity);
			}
			
			RemoveSelf();
		}

		private Player player;

		private void BindEntity(Type type, Entity entity)
		{
			if (wraptype.Contains(type))
			{
				foreach (var typex in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField)
					.Where((x) => x.FieldType == typeof(Holdable)))
				{
					var orig_pickup = (typex.GetValue(entity) as Holdable).OnPickup;
					(typex.GetValue(entity) as Holdable).OnPickup = () =>
					{
						orig_pickup();
						player = (typex.GetValue(entity) as Holdable).Holder;
					};
					var orig_release = (typex.GetValue(entity) as Holdable).OnRelease;
					(typex.GetValue(entity) as Holdable).OnRelease = (x) =>
					{
						orig_release(x);
						if (x.X != 0f)
						{
							if (player == null)
							{
								return;
							}
							//player.Speed.X += 80f * (float)player.Facing;
							player.Speed.X += pushForce * (float)-(float)player.Facing;
						}
					};
				}
			}
		}
	}
}