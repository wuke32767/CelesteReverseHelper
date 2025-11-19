using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;
using Monocle;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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
        public bool Refilled
        {
            get => _refilled;
            set
            {
                _refilled = value;
                if (Entity is null || cache is null)
                {
                    return;
                }
                cache.enable = Refilled;
                cache.disable = !Refilled;
                DreamToggleListener.ForceUpdateSingle(Entity.Scene as Level, Entity);
            }
        }
        public override void Update()
        {
            base.Update();
        }
        public void Deactivate()
        {
            if (!Refilled)
            {
                return;
            }
            Refilled = false;
        }
        public void Activate()
        {
            if (Refilled)
            {
                return;
            }
            Refilled = true;
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
            cache ??= DreamBlockConfig.GetOrAdd(Entity);
            Refilled = _refilled;
        }
        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            cache = null;
        }
    }
    [CustomEntity("ReverseHelper/RefillDreamBlockRefill")]
    public class RefillDreamBlockRefill : Refill
    {
        public RefillDreamBlockRefill(Vector2 pos, bool oneuse, float respawn, string text) : base(pos, false, oneuse)
        {
            Get<PlayerCollider>().OnCollide = p =>
            {
                if (UseRefill())
                {
                    Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", Position);
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    Collidable = false;
                    Add(new Coroutine(RefillRoutine(p)));
                    respawnTimer = respawn;
                }
            };
            sprite.Reset(GFX.Game, text + "idle");
            outline.Texture = GFX.Game[text + "outline"];
            flash.Reset(GFX.Game, text + "flash");
        }

        private bool UseRefill()
        {
            bool ret = false;
            foreach (NeedRefillDreamBlockContainerComponent i in Scene.Tracker.GetComponents<NeedRefillDreamBlockContainerComponent>())
            {
                if (!i.Refilled)
                {
                    i.Refilled = true;
                    ret = true;
                }
            }
            return ret;
        }

        public RefillDreamBlockRefill(EntityData e, Vector2 offset)
            : this(e.Position + offset, e.Bool("oneUse"),
                  e.Float("respawnTime", 2.5f), e.Attr("image", "objects/refill/"))
        {
        }

    }
}