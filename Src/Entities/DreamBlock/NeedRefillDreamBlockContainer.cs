using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;
using Monocle;
using System.Runtime.CompilerServices;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [CustomEntity("ReverseHelper/NeedRefillDreamBlockContainer")]
    public class NeedRefillDreamBlockContainer(Vector2 position, float width, float height) : Entity(position)
    {
        public NeedRefillDreamBlockContainer(EntityData e, Vector2 offset) : this(e.Position + offset, e.Width, e.Height)
        {
        }
        public override void Added(Scene scene)
        {
            Collider = new Hitbox(width, height);
            base.Added(scene);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (Entity playerCollider in this.CollidableAll<DreamBlock>())
            {
                if (playerCollider.Collider.Collide(Collider))
                {
                    BindEntity(playerCollider);
                }
            }
            //foreach (var (t, _) in DreamBlockConfigurer.ExternalDreamBlockLike)
            //{
            //    if (Scene.Tracker.Entities.TryGetValue(t, out var list))
            //    {
            //        foreach (Entity playerCollider in list)
            //        {
            //            if (playerCollider.Collider.Collide(Collider))
            //            {
            //                BindEntity(playerCollider);
            //            }
            //        }
            //    }
            //}
            foreach (var (t, g) in DreamBlockConfigurer.ExternalDreamBlockDummy)
            {
                if (Scene.Tracker.Entities.TryGetValue(t, out var list))
                {
                    foreach (Entity playerCollider in list)
                    {
                        if (playerCollider.Collider.Collide(Collider))
                        {
                            BindEntity(g(playerCollider));
                        }
                    }
                }
            }

            RemoveSelf();
        }
        private void BindEntity(Entity playerCollider)
        {
            playerCollider.Add(new NeedRefillDreamBlockContainerComponent());
        }
    }
    [Tracked]
    public class NeedRefillDreamBlockContainerComponent() : Component(true, false)
    {
        public static bool has = false;
        bool _refilled = true;
        DreamBlockConfig? cache;
        bool refilled
        {
            get => _refilled;
            set
            {
                _refilled = value;
                if (Entity is null)
                {
                    return;
                }
                cache ??= DreamBlockConfig.GetOrAdd(Entity);
                cache.enable = refilled;
                cache.disable = !refilled;
                DreamToggleListener.ForceUpdateSingle(Entity.Scene as Level, Entity);
            }
        }
        public override void Update()
        {
            base.Update();
        }
        public void Deactivate()
        {
            if (!refilled)
            {
                return;
            }
            refilled = false;
        }
        public void Activate()
        {
            if (refilled)
            {
                return;
            }
            refilled = true;
        }

        private DreamBlock? EntityAs => Entity as DreamBlock;

        [SourceGen.Loader.Load]
        public static void Load()
        {
            On.Celeste.Player.DreamDashBegin += Player_DreamDashBegin;
        }

        private static void Player_DreamDashBegin(On.Celeste.Player.orig_DreamDashBegin orig, Player self)
        {
            orig(self);
            if (self.dreamBlock?.Get<NeedRefillDreamBlockContainerComponent>() is { } cm)
            {
                cm.Deactivate();
            }
        }

        [SourceGen.Loader.Unload]
        public static void UnLoad()
        {
            On.Celeste.Player.DreamDashBegin -= Player_DreamDashBegin;
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            refilled = _refilled;
        }
    }
    [CustomEntity("ReverseHelper/RefillDreamBlockRefill")]
    public class RefillDreamBlockRefill : Refill
    {
        public RefillDreamBlockRefill(Vector2 position, bool oneUse) : base(position, false, oneUse)
        {
        }

        public RefillDreamBlockRefill(EntityData e, Vector2 offset) : this(e.Position + offset, e.Bool("oneUse", false))
        {
        }
    }
}