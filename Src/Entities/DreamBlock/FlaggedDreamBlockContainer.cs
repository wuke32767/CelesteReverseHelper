﻿using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [CustomEntity("ReverseHelper/FlaggedDreamBlockContainer")]
    public class FlaggedDreamBlockContainer(Vector2 position, float width, float height, string flag) : Entity(position)
    {
        public FlaggedDreamBlockContainer(EntityData e, Vector2 offset) : this(e.Position + offset, e.Width, e.Height, e.Attr("flag"))
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
            foreach (var (t, _) in DreamBlockConfigurer.ExternalDreamBlockLike)
            {
                if (Scene.Tracker.Entities.TryGetValue(t, out var list))
                {
                    foreach (Entity playerCollider in list)
                    {
                        if (playerCollider.Collider.Collide(Collider))
                        {
                            BindEntity(playerCollider);
                        }
                    }
                }
            }
            RemoveSelf();
        }
        private void BindEntity(Entity playerCollider)
        {
            playerCollider.Add(new FlaggedDreamBlockContainerComponent(flag));
        }
    }
    [Tracked]
    public class FlaggedDreamBlockContainerComponent(string flag) : Component(true, false)
    {
        FlagMatch Flag = flag;
        bool Match => Flag.IsMatch(SceneAs<Level>(), true);
        bool last;
        public override void Update()
        {
            base.Update();
            UpdateState();
        }
        public void UpdateState()
        {
            var hh = DreamBlockConfig.GetOrAdd(Entity);
            bool match = Match;
            if (last != match)
            {
                hh.enable = match;
                hh.disable = !match;
                last = match;
                DreamToggleListener.ForceUpdateSingle(SceneAs<Level>(), Entity);
            }
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            last = !Match;
            UpdateState();
        }
    }

}