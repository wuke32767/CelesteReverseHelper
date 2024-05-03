using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [CustomEntity("ReverseHelper/MomentumBumper")]
    public class MomentumBumper : Entity
    {
        public float MomentumFactor;
        // Token: 0x06000DC9 RID: 3529 RVA: 0x00030EB0 File Offset: 0x0002F0B0
        public MomentumBumper(Vector2 position, Vector2? node, float momentumFactor) : base(position)
        {
            Collider = new Circle(12f, 0f, 0f);
            Add(new PlayerCollider(OnPlayer, null, null));
            Add(sine = new SineWave(0.44f, 0f).Randomize());
            Add(sprite = GFX.SpriteBank.Create("bumper"));
            Add(spriteEvil = GFX.SpriteBank.Create("bumper_evil"));
            spriteEvil.Visible = false;
            Add(light = new VertexLight(Color.Teal, 1f, 16, 32));
            Add(bloom = new BloomPoint(0.5f, 16f));
            this.node = node;
            anchor = Position;
            if (node != null)
            {
                Vector2 start = Position;
                Vector2 end = node.Value;
                Tween tween = Tween.Create(Tween.TweenMode.Looping, Ease.CubeInOut, 1.81818187f, true);
                tween.OnUpdate = delegate (Tween t)
                {
                    if (goBack)
                    {
                        anchor = Vector2.Lerp(end, start, t.Eased);
                        return;
                    }
                    anchor = Vector2.Lerp(start, end, t.Eased);
                };
                tween.OnComplete = delegate (Tween t)
                {
                    goBack = !goBack;
                };
                Add(tween);
            }
            UpdatePosition();
            Add(hitWiggler = Wiggler.Create(1.2f, 2f, delegate (float v)
            {
                spriteEvil.Position = hitDir * hitWiggler!.Value * 8f;
            }, false, false));
            Add(new CoreModeListener(OnChangeMode));
            MomentumFactor = momentumFactor;
        }

        // Token: 0x06000DCA RID: 3530 RVA: 0x00031075 File Offset: 0x0002F275
        public MomentumBumper(EntityData data, Vector2 offset) : this(data.Position + offset, data.FirstNodeNullable(new Vector2?(offset)), data.Float("momentumFactor", -1))
        {
        }

        // Token: 0x06000DCB RID: 3531 RVA: 0x00031098 File Offset: 0x0002F298
        public override void Added(Scene scene)
        {
            base.Added(scene);
            fireMode = SceneAs<Level>().CoreMode == Session.CoreModes.Hot;
            spriteEvil.Visible = fireMode;
            sprite.Visible = !fireMode;
        }

        // Token: 0x06000DCC RID: 3532 RVA: 0x000310E5 File Offset: 0x0002F2E5
        private void OnChangeMode(Session.CoreModes coreMode)
        {
            fireMode = coreMode == Session.CoreModes.Hot;
            spriteEvil.Visible = fireMode;
            sprite.Visible = !fireMode;
        }

        // Token: 0x06000DCD RID: 3533 RVA: 0x00031116 File Offset: 0x0002F316
        private void UpdatePosition()
        {
            Position = anchor + new Vector2(sine.Value * 3f, sine.ValueOverTwo * 2f);
        }

        // Token: 0x06000DCE RID: 3534 RVA: 0x00031150 File Offset: 0x0002F350
        public override void Update()
        {
            base.Update();
            if (respawnTimer > 0f)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f)
                {
                    light.Visible = true;
                    bloom.Visible = true;
                    sprite.Play("on", false, false);
                    spriteEvil.Play("on", false, false);
                    if (!fireMode)
                    {
                        Audio.Play("event:/game/06_reflection/pinballbumper_reset", Position);
                    }
                }
            }
            else if (Scene.OnInterval(0.05f))
            {
                float num = Calc.Random.NextAngle();
                ParticleType type = fireMode ? P_FireAmbience : P_Ambience;
                float direction = fireMode ? -1.57079637f : num;
                float length = (float)(fireMode ? 12 : 8);
                SceneAs<Level>().Particles.Emit(type, 1, Center + Calc.AngleToVector(num, length), Vector2.One * 2f, direction);
            }
            UpdatePosition();
        }

        // Token: 0x06000DCF RID: 3535 RVA: 0x0003127C File Offset: 0x0002F47C
        private void OnPlayer(Player player)
        {
            if (fireMode)
            {
                if (!SaveData.Instance.Assists.Invincible)
                {
                    Vector2 vector = (player.Center - Center).SafeNormalize();
                    hitDir = -vector;
                    hitWiggler.Start();
                    Audio.Play("event:/game/09_core/hotpinball_activate", Position);
                    respawnTimer = 0.6f;
                    player.Die(vector, false, true);
                    SceneAs<Level>().Particles.Emit(P_FireHit, 12, Center + vector * 12f, Vector2.One * 3f, vector.Angle());
                    return;
                }
            }
            else if (respawnTimer <= 0f)
            {
                if (SceneAs<Level>().Session.Area.ID == 9)
                {
                    Audio.Play("event:/game/09_core/pinballbumper_hit", Position);
                }
                else
                {
                    Audio.Play("event:/game/06_reflection/pinballbumper_hit", Position);
                }
                respawnTimer = 0.6f;
                Vector2 vector2 = ExplodeLaunchMomentum(player, Position, MomentumFactor, false, false);
                sprite.Play("hit", true, false);
                spriteEvil.Play("hit", true, false);
                light.Visible = false;
                bloom.Visible = false;
                SceneAs<Level>().DirectionalShake(vector2, 0.15f);
                SceneAs<Level>().Displacement.AddBurst(Center, 0.3f, 8f, 32f, 0.8f, null, null);
                SceneAs<Level>().Particles.Emit(P_Launch, 12, Center + vector2 * 12f, Vector2.One * 3f, vector2.Angle());
            }
        }

        // Token: 0x04000901 RID: 2305
        public static ParticleType P_Ambience { get => Bumper.P_Ambience; }

        // Token: 0x04000902 RID: 2306
        public static ParticleType P_Launch { get => Bumper.P_Launch; }

        // Token: 0x04000903 RID: 2307
        public static ParticleType P_FireAmbience { get => Bumper.P_FireAmbience; }

        // Token: 0x04000904 RID: 2308
        public static ParticleType P_FireHit { get => Bumper.P_FireHit; }

        // Token: 0x04000905 RID: 2309
        private const float RespawnTime = 0.6f;

        // Token: 0x04000906 RID: 2310
        private const float MoveCycleTime = 1.81818187f;

        // Token: 0x04000907 RID: 2311
        private const float SineCycleFreq = 0.44f;

        // Token: 0x04000908 RID: 2312
        private Sprite sprite;

        // Token: 0x04000909 RID: 2313
        private Sprite spriteEvil;

        // Token: 0x0400090A RID: 2314
        private VertexLight light;

        // Token: 0x0400090B RID: 2315
        private BloomPoint bloom;

        // Token: 0x0400090C RID: 2316
        private Vector2? node;

        // Token: 0x0400090D RID: 2317
        private bool goBack;

        // Token: 0x0400090E RID: 2318
        private Vector2 anchor;

        // Token: 0x0400090F RID: 2319
        private SineWave sine;

        // Token: 0x04000910 RID: 2320
        private float respawnTimer;

        // Token: 0x04000911 RID: 2321
        private bool fireMode;

        // Token: 0x04000912 RID: 2322
        private Wiggler hitWiggler;

        // Token: 0x04000913 RID: 2323
        private Vector2 hitDir;
        public static Vector2 ExplodeLaunchMomentum(Player player, Vector2 from, float momentumfactor, bool snapUp = true, bool sidesOnly = false)
        {
            var pl = new DynamicData(typeof(Player), player);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Celeste.Freeze(0.1f);
            pl.Set("launchApproachX", null);
            //player.launchApproachX = null;
            Vector2 vector = (player.Center - from).SafeNormalize(-Vector2.UnitY);
            float num = Vector2.Dot(vector, Vector2.UnitY);
            if (snapUp && num <= -0.7f)
            {
                vector.X = 0f;
                vector.Y = -1f;
            }
            else if (num <= 0.65f && num >= -0.55f)
            {
                vector.Y = 0f;
                vector.X = (float)Math.Sign(vector.X);
            }
            if (sidesOnly && vector.X != 0f)
            {
                vector.Y = 0f;
                vector.X = (float)Math.Sign(vector.X);
            }
            player.Speed *= momentumfactor;
            player.Speed += 280f * vector;
            if (player.Speed.Y <= 50f)
            {
                player.Speed.Y = Math.Min(-150f, player.Speed.Y);
                player.AutoJump = true;
            }
            if (player.Speed.X != 0f)
            {
                if (Input.MoveX.Value == Math.Sign(player.Speed.X))
                {
                    pl.Set("explodeLaunchBoostTimer", 0f);
                    //player.explodeLaunchBoostTimer = 0f;
                    player.Speed.X = player.Speed.X * 1.2f;
                }
                else
                {
                    pl.Set("explodeLaunchBoostTimer", 0.01f);
                    //player.explodeLaunchBoostTimer = 0.01f;
                    pl.Set("explodeLaunchBoostSpeed", player.Speed.X * 1.2f);
                    //player.explodeLaunchBoostSpeed = player.Speed.X * 1.2f;
                }
            }
            SlashFx.Burst(player.Center, player.Speed.Angle());
            if (!player.Inventory.NoRefills)
            {
                player.RefillDash();
            }
            player.RefillStamina();
            pl.Set("dashCooldownTimer", 0.2f);
            //player.dashCooldownTimer = 0.2f;
            player.StateMachine.State = 7;
            return vector;
        }

    }

}