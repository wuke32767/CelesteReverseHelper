using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [CustomEntity("ReverseHelper/DreamToggle")]
    public class DreamToggle : Entity
    {
        public DreamToggle(Vector2 position, bool onlyFire, bool onlyIce,bool persistent) : base(position)
        {
            this.onlyDisable = onlyFire;
            this.onlyEnable = onlyIce;
            this.persistent = persistent = true;
            base.Collider = new Hitbox(16f, 24f, -8f, -12f);
            base.Add(new DreamToggleListener((OnChangeMode)));
            base.Add(new PlayerCollider((OnPlayer), null, null));
            base.Add(sprite = GFX.SpriteBank.Create("ReverseHelper_DreamToggle"));
            base.Depth = 2000;
        }

        // Token: 0x0600164F RID: 5711 RVA: 0x0005C3BA File Offset: 0x0005A5BA
        public DreamToggle(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("onlyEnable", false), data.Bool("onlyDisable", false), data.Bool("persistent_always_true", true))
        {
        }

        // Token: 0x06001650 RID: 5712 RVA: 0x0005C3F2 File Offset: 0x0005A5F2

        public override void Added(Scene scene)
        {
            base.Added(scene);
            enableMode = (ReverseHelperModule.playerHasDreamDash );
            SetSprite(false);
        }

        // Token: 0x06001651 RID: 5713 RVA: 0x0005C416 File Offset: 0x0005A616
        private void OnChangeMode(bool enabled)
        {
            enableMode = (ReverseHelperModule.playerHasDreamDash);
            SetSprite(true);
        }

        // Token: 0x06001652 RID: 5714 RVA: 0x0005C42C File Offset: 0x0005A62C

        private void SetSprite(bool animate)
        {
            if (animate)
            {
                if (playSounds)
                {
                    Audio.Play(enableMode ? "event:/game/09_core/switch_to_cold" : "event:/game/09_core/switch_to_hot", Position);
                }
                if (Usable)
                {
                    sprite.Play(enableMode ? "enable" : "disable", false, false);
                }
                else
                {
                    if (playSounds)
                    {
                        Audio.Play("event:/game/09_core/switch_dies", Position);
                    }
                    sprite.Play(enableMode ? "enableOff" : "disableOff", false, false);
                }
            }
            else if (Usable)
            {
                sprite.Play(enableMode ? "enableLoop" : "disableLoop", false, false);
            }
            else
            {
                sprite.Play(enableMode ? "enableOffLoop" : "disableOffLoop", false, false);
            }
            playSounds = false;
        }

        // Token: 0x06001653 RID: 5715 RVA: 0x0005C524 File Offset: 0x0005A724

        private void OnPlayer(Player player)
        {
            if (Usable && cooldownTimer <= 0f)
            {
                playSounds = true;
                Level level = base.SceneAs<Level>();

                //if (level.CoreMode == Session.CoreModes.Cold)
                //{
                //    level.CoreMode = Session.CoreModes.Hot;
                //}
                //else
                //{
                //    level.CoreMode = Session.CoreModes.Cold;
                //}
                //if (persistent)
                //{
                //    level.Session.CoreMode = level.CoreMode;
                //}

                ReverseHelperModule.playerHasDreamDash = !ReverseHelperModule.playerHasDreamDash;

                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                level.Flash(Color.White * 0.15f, true);
                Celeste.Freeze(0.05f);
                cooldownTimer = 1f;
                DreamToggleListener.ImmediateUpdate();
            }
        }

        // Token: 0x06001654 RID: 5716 RVA: 0x0005C5BB File Offset: 0x0005A7BB

        public override void Update()
        {
            base.Update();
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Engine.DeltaTime;
            }
        }

        // Token: 0x17000340 RID: 832
        // (get) Token: 0x06001655 RID: 5717 RVA: 0x0005C5E2 File Offset: 0x0005A7E2
        private bool Usable
        {

            get
            {
                return (!onlyDisable || !enableMode) && (!onlyEnable || enableMode);
            }
        }

        // Token: 0x04000F79 RID: 3961
        private const float Cooldown = 1f;

        // Token: 0x04000F7A RID: 3962
        private bool enableMode;

        // Token: 0x04000F7B RID: 3963
        private float cooldownTimer;

        // Token: 0x04000F7C RID: 3964
        private bool onlyDisable;

        // Token: 0x04000F7D RID: 3965
        private bool onlyEnable;

        // Token: 0x04000F7E RID: 3966
        private bool persistent;

        // Token: 0x04000F7F RID: 3967
        private bool playSounds;

        // Token: 0x04000F80 RID: 3968
        private Sprite sprite;
    }
}