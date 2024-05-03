using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
#pragma warning disable
namespace Celeste.Mod.ReverseHelper.Entities
{
    public static class LongDashRefillInst
    {
        private static float _dashtime;
        public static float dashTime { set => _dashtime = value / 0.3f; get => _dashtime; }
        public static bool enable = false;
        public static bool disableSpring = true;
        public static void Load()
        {
            sr = ReverseHelperExtern.SpeedRunTool_Interop.RegisterStaticTypes?.Invoke(typeof(DreamToggleListener), [nameof(_dashtime),nameof(enable),nameof(disableSpring)]);
        }
        static object? sr;
        public static void Unload()
        {
            ReverseHelperExtern.SpeedRunTool_Interop.Unregister?.Invoke(sr!);
        }

    }

    [CustomEntity("ReverseHelper/LongDashRefill")]
    public class LongDashRefill : Entity
    {
        private bool disableSpring = true;
        [SourceGen.Loader.Load]

        public static void Load()
        {
            On.Celeste.Player.DashBegin += DashBegin;
            On.Celeste.Player.ctor += Player_ctor;
        }

        private static void Player_ctor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
        {
            orig(self, position, spriteMode);
            LongDashRefillInst.enable = false;
        }

        //private static ExtendedVariantsModule.Variant variant = (ExtendedVariantsModule.Variant)Enum.Parse(typeof(ExtendedVariantsModule.Variant), "DashLength");

        private static IEnumerator what_after_dash_begin()
        {
            yield return 0.01f;
            //ExtendedVariantsModule.Instance.TriggerManager.OnExitedRevertOnLeaveTrigger(variant, LongDashRefillInst.dashTime, legacy: false);
            ReverseHelperExtern.ExtendedVariantsModule.Interop.TriggerFloatVariant("DashLength", 1, true);
        }

        private static System.Reflection.FieldInfo varJumpTimerRefl = typeof(Player).GetField("varJumpTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField);

        private static void DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
        {
            if (LongDashRefillInst.enable)
            {
                LongDashRefillInst.enable = false;
                //origval = (float)ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(variant);
                orig(self);
                self.Add(new Coroutine(what_after_dash_begin()));
                if (LongDashRefillInst.disableSpring)
                {
                    //self.AutoJump = false;
                    //self.AutoJumpTimer = 0f;
                    varJumpTimerRefl.SetValue(self, 0f);
                }
            }
            else
            {
                orig(self);
            }
        }

        [SourceGen.Loader.Unload]
        public static void Unload()
        {
            On.Celeste.Player.DashBegin -= DashBegin;
        }

        private string img;

        // Token: 0x06002F24 RID: 12068 RVA: 0x001160A0 File Offset: 0x001142A0
        public LongDashRefill(Vector2 position, bool oneUse, float dashtime, string img) : base(position)
        {
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(new Action<Player>(OnPlayer), null, null));
            this.oneUse = oneUse;

            p_shatter = Refill.P_Shatter;
            p_regen = Refill.P_Regen;
            p_glow = Refill.P_Glow;

            this.img = img;

            ;
            Add(outline = new Image(GFX.Game[img + "outline"]));
            outline.CenterOrigin();
            outline.Visible = false;
            Add(sprite = new Sprite(GFX.Game, img + "idle"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle", false, false);
            sprite.CenterOrigin();
            Add(flash = new Sprite(GFX.Game, img + "flash"));
            flash.Add("flash", "", 0.05f);
            flash.OnFinish = delegate (string anim)
            {
                flash.Visible = false;
            };
            flash.CenterOrigin();
            Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v)
            {
                sprite.Scale = (flash.Scale = Vector2.One * (1f + v * 0.2f));
            }, false, false));
            Add(new MirrorReflection());
            Add(bloom = new BloomPoint(0.8f, 16f));
            Add(light = new VertexLight(Color.White, 1f, 16, 48));
            Add(sine = new SineWave(0.6f, 0f));
            sine.Randomize();
            UpdateY();
            Depth = -100;
            dashTime = dashtime;
        }

        // Token: 0x06002F25 RID: 12069 RVA: 0x00116300 File Offset: 0x00114500
        public LongDashRefill(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("oneUse", false), data.Float("dashTime", 0.01f), data.Attr("image", "objects/ReverseHelper/LongDashRefill/"))
        {
        }

        // Token: 0x06002F26 RID: 12070 RVA: 0x0011632C File Offset: 0x0011452C
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        // Token: 0x06002F27 RID: 12071 RVA: 0x00116344 File Offset: 0x00114544
        public override void Update()
        {
            base.Update();
            if (respawnTimer > 0f)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f)
                {
                    Respawn();
                }
            }
            else if (Scene.OnInterval(0.1f))
            {
                level.ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
            }
            UpdateY();
            light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
            bloom.Alpha = light.Alpha * 0.8f;
            if (Scene.OnInterval(2f) && sprite.Visible)
            {
                flash.Play("flash", true, false);
                flash.Visible = true;
            }
        }

        // Token: 0x06002F28 RID: 12072 RVA: 0x00116468 File Offset: 0x00114668
        private void Respawn()
        {
            if (!Collidable)
            {
                Collidable = true;
                sprite.Visible = true;
                outline.Visible = false;
                Depth = -100;
                wiggler.Start();
                Audio.Play("event:/game/general/diamond_return", Position);
                level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
            }
        }

        // Token: 0x06002F29 RID: 12073 RVA: 0x00116500 File Offset: 0x00114700
        private void UpdateY()
        {
            flash.Y = (sprite.Y = (bloom.Y = sine.Value * 2f));
        }

        // Token: 0x06002F2A RID: 12074 RVA: 0x00116545 File Offset: 0x00114745
        public override void Render()
        {
            if (sprite.Visible)
            {
                sprite.DrawOutline(1);
            }
            base.Render();
        }

        // Token: 0x06002F2B RID: 12075 RVA: 0x00116568 File Offset: 0x00114768
        private void OnPlayer(Player player)
        {
            if (player.UseRefill(false) || !LongDashRefillInst.enable)
            {
                Audio.Play("event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;
                Add(new Coroutine(RefillRoutine(player), true));
                respawnTimer = 2.5f;
                LongDashRefillInst.enable = true;
                LongDashRefillInst.dashTime = dashTime;
                LongDashRefillInst.disableSpring = disableSpring;
                ReverseHelperExtern.ExtendedVariantsModule.Interop.TriggerFloatVariant("DashLength", LongDashRefillInst.dashTime, true);
            }
        }

        // Token: 0x06002F2C RID: 12076 RVA: 0x001165CF File Offset: 0x001147CF
        private IEnumerator RefillRoutine(Player player)
        {
            Celeste.Freeze(0.05f);
            yield return null;
            level.Shake(0.3f);
            sprite.Visible = flash.Visible = false;
            if (!oneUse)
            {
                outline.Visible = true;
            }
            Depth = 8999;
            yield return 0.05f;
            float num = player.Speed.Angle();
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - 1.57079637f);
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + 1.57079637f);
            SlashFx.Burst(Position, num);
            if (oneUse)
            {
                RemoveSelf();
            }
            yield break;
        }

        // Token: 0x04002887 RID: 10375
        public static ParticleType P_Shatter { get => Refill.P_Shatter; }

        // Token: 0x04002888 RID: 10376
        public static ParticleType P_Regen { get => Refill.P_Regen; }

        // Token: 0x04002889 RID: 10377
        public static ParticleType P_Glow { get => Refill.P_Glow; }

        // Token: 0x0400288A RID: 10378
        public static ParticleType P_ShatterTwo { get => Refill.P_ShatterTwo; }

        // Token: 0x0400288B RID: 10379
        public static ParticleType P_RegenTwo { get => Refill.P_RegenTwo; }

        // Token: 0x0400288C RID: 10380
        public static ParticleType P_GlowTwo { get => Refill.P_GlowTwo; }

        // Token: 0x0400288D RID: 10381
        private Sprite sprite;

        // Token: 0x0400288E RID: 10382
        private Sprite flash;

        // Token: 0x0400288F RID: 10383
        private Image outline;

        // Token: 0x04002890 RID: 10384
        private Wiggler wiggler;

        // Token: 0x04002891 RID: 10385
        private BloomPoint bloom;

        // Token: 0x04002892 RID: 10386
        private VertexLight light;

        // Token: 0x04002893 RID: 10387
        private Level level;

        // Token: 0x04002894 RID: 10388
        private SineWave sine;

        // Token: 0x04002896 RID: 10390
        private bool oneUse;

        // Token: 0x04002897 RID: 10391
        private ParticleType p_shatter;

        // Token: 0x04002898 RID: 10392
        private ParticleType p_regen;

        // Token: 0x04002899 RID: 10393
        private ParticleType p_glow;

        // Token: 0x0400289A RID: 10394
        private float respawnTimer;

        private float dashTime;
    }
}
#pragma warning restore