using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using Component = Monocle.Component;

namespace Celeste.Mod.ReverseHelper.Entities
{
    public class ForceyHoldablesComponent : Component
    {
        public new Level Scene { get => base.Scene as Level; }
        private Dictionary<FlagMatch, float> flagControled = new Dictionary<FlagMatch, float>();

        public ForceyHoldablesComponent() : base(true, false)
        {
        }

        public ForceyHoldablesComponent(float force) : base(true, false)
        {
            _force = force;
        }

        public override void Update()
        {
            base.Update();
        }

        public void Add(float val)
        {
            _force += val;
        }

        public void Add(FlagMatch flag, float val)
        {
            if (flagControled.TryGetValue(flag, out float v2))
            {
                v2 += val;
                flagControled[flag] = v2;
            }
            else
            {
                flagControled.Add(flag, val);
            }
        }

        public float _force;

        public float force
        {
            get
            {
                var fc = _force;
                foreach (var kvp in flagControled)
                {
                    if (kvp.Key.IsMatch(Scene, true))
                    {
                        fc += kvp.Value;
                    }
                }
                return fc;
            }
        }
    }

    public static class ForceyHoldablesComponentPlayer
    {
        public static float force = 0f;
        public static void Load()
        {
            sr = ReverseHelperExtern.SpeedRunTool_Interop.RegisterStaticTypes?.Invoke(typeof(DreamToggleListener), [nameof(force)]);
        }
        static object? sr;
        public static void Unload()
        {
            ReverseHelperExtern.SpeedRunTool_Interop.Unregister?.Invoke(sr!);
        }

    }

    [CustomEntity("ReverseHelper/ForceyHoldables")]
    public class ForceyHoldables : Entity
    {
        public new Level Scene { get => base.Scene as Level; }
        private bool bindOnRoomStart;
        private FlagMatch flagBind;
        private FlagMatch flagEnable;
        private TypeMatch wraptype;
        private float pushForce;

        public override void Update()
        {
            base.Update();
            if (flagBind.IsMatch(Scene as Level, false))
            {
                foreach (Holdable playerCollider in CollideAllByComponent<Holdable>())
                {
                    BindEntity(playerCollider.Entity.GetType(), playerCollider.Entity);
                }
            }
        }

        public ForceyHoldables(Vector2 position, float width, float height, string typename, float force, string flagBind, string flagEnable, bool bindOnRoomStart) : base(position)
        {
            Collider = new Hitbox(width, height, 0f, 0f);
            wraptype = typename;
            pushForce = force;
            this.flagBind = flagBind;
            this.flagEnable = flagEnable;
            this.bindOnRoomStart = bindOnRoomStart;
        }

        // Token: 0x06000EF3 RID: 3827 RVA: 0x0003A0EC File Offset: 0x000382EC
        public ForceyHoldables(EntityData e, Vector2 offset) : this(e.Position + offset, e.Width, e.Height, e.Attr("type", "Celeste.Grider"), e.Float("force", 80), e.Attr("flagBind", ""), e.Attr("flagEnabled", ""), e.Bool("bindOnRoomStart", true))
        {
        }
        [SourceGen.Loader.Load]
        public static void Load()
        {
            On.Celeste.Player.Throw += ForceyHoldableOnThrow;
        }

        public static void ForceyHoldableOnThrow(On.Celeste.Player.orig_Throw orig_Throw, Player self)
        {
            if (self.Holding != null)
            {
                if (Input.MoveY.Value != 1)
                {
                    var fc = self.Holding.Entity.Get<ForceyHoldablesComponent>();
                    self.Speed.X += (fc?.force ?? 0f + ForceyHoldablesComponentPlayer.force) * (float)(-(float)self.Facing);
                    //self.Speed.X = self.Speed.X + 80f * (float)self.Facing;
                }
            }
            orig_Throw(self);
        }

        [SourceGen.Loader.Unload]
        public static void Unload()
        {
            On.Celeste.Player.Throw -= ForceyHoldableOnThrow;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (bindOnRoomStart)
            {
                foreach (Holdable playerCollider in CollideAllByComponent<Holdable>())
                {
                    BindEntity(playerCollider.Entity.GetType(), playerCollider.Entity);
                }
            }

            //if (flagBind.Empty())
            //{
            //    RemoveSelf();
            //}
        }

        private void BindEntity(Type type, Entity entity)
        {
            if (wraptype.Contains(type))
            {
                ForceyHoldablesComponent fc = entity.Get<ForceyHoldablesComponent>();
                if (fc is null)
                {
                    entity.Add(fc = new ForceyHoldablesComponent());
                }
                fc.Add(flagEnable, pushForce);
            }
        }
    }
}