#pragma warning disable CS8618
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [CustomEntity("ReverseHelper/BumperSeeker")]
    public class BumperSeeker : Seeker
    {

        public BumperSeeker(EntityData data, Vector2 offset)
            : base(data, offset)
        {

            Add(bumpersprite = GFX.SpriteBank.Create("bumper"));
            Add(bumperspriteEvil = GFX.SpriteBank.Create("bumper_evil"));
            bumperspriteEvil.Visible = false;
            Add(light = new VertexLight(Color.Teal, 1f, 16, 32));
            Add(bloom = new BloomPoint(0.5f, 16f));
            this.node = null;
            anchor = Position;
            if (node.HasValue)
            {
            }

            Add(hitWiggler = Wiggler.Create(1.2f, 2f, (float v) =>
            {
                bumperspriteEvil.Position = hitDir * hitWiggler.Value * 8f;
            }));
            Add(new CoreModeListener(OnBumperChangeMode));


            var coll = Components.GetAll<PlayerCollider>().ToArray();
            foreach (var c in coll)
            {
                if (c.Collider == bounceHitbox)
                {
                    c.OnCollide = player =>
                    {
                        if (fireMode)
                        {
                            if (!SaveData.Instance.Assists.Invincible)
                            {
                                Vector2 vector = (player.Center - base.Center).SafeNormalize();
                                hitDir = -vector;
                                hitWiggler.Start();
                                Audio.Play("event:/game/09_core/hotpinball_activate", Position);
                                respawnTimer = 0.6f;
                                player.Die(vector);
                                SceneAs<Level>().Particles.Emit(Bumper.P_FireHit, 12, base.Center + vector * 12f, Vector2.One * 3f, vector.Angle());
                            }
                        }
                        else if (respawnTimer <= 0f)
                        {
                            if ((base.Scene as Level).Session.Area.ID == 9)
                            {
                                Audio.Play("event:/game/09_core/pinballbumper_hit", Position);
                            }
                            else
                            {
                                Audio.Play("event:/game/06_reflection/pinballbumper_hit", Position);
                            }

                            respawnTimer = 0.6f;
                            Vector2 vector2 = player.ExplodeLaunch(Position, snapUp: false, sidesOnly: false);
                            bumpersprite.Play("hit", restart: true);
                            bumperspriteEvil.Play("hit", restart: true);
                            light.Visible = false;
                            bloom.Visible = false;
                            SceneAs<Level>().DirectionalShake(vector2, 0.15f);
                            SceneAs<Level>().Displacement.AddBurst(base.Center, 0.3f, 8f, 32f, 0.8f);
                            SceneAs<Level>().Particles.Emit(Bumper.P_Launch, 12, base.Center + vector2 * 12f, Vector2.One * 3f, vector2.Angle());


                            //Collider collider = base.Collider;
                            //base.Collider = bounceHitbox;
                            //player.PointBounce(base.Center);
                            Speed = (base.Center - player.Center).SafeNormalize(100f);
                            if (State == 3)
                            {
                                Speed = -Speed / 2;
                                facing = -facing;
                                State.State = StSkidding;
                            }
                            scaleWiggler.Start();
                            //base.Collider = collider;

                        }

                    };
                    c.Collider = new Circle(12f);
                }
                else if (c.Collider == attackHitbox)
                {
                    Remove(c);
                }
            }
            State.SetCallbacks(3, AttackUpdate, AttackCoroutine, () =>
            {
                AttackBegin();
                //attackSpeed = -30f;
                //Speed = (FollowTarget - base.Center).SafeNormalize(attackSpeed);
            });
        }


        public override void Render()
        {
            Vector2 position = Position;
            Position += shaker.Value;
            Vector2 scale = bumpersprite.Scale;
            bumpersprite.Scale *= 1f - 0.3f * scaleWiggler.Value;
            bumpersprite.Scale.X *= spriteFacing;
            base.Render();
            Position = position;
            bumpersprite.Scale = scale;
        }

        public override void DebugRender(Camera camera)
        {
            Collider collider = base.Collider;
            base.Collider = attackHitbox;
            attackHitbox.Render(camera, Color.Red);
            base.Collider = bounceHitbox;
            bounceHitbox.Render(camera, Color.Aqua);
            base.Collider = collider;
        }
        public const float RespawnTime = 0.6f;

        public const float MoveCycleTime = 1.81818187f;

        public const float SineCycleFreq = 0.44f;

        public Sprite bumpersprite;

        public Sprite bumperspriteEvil;

        public VertexLight light;

        public BloomPoint bloom;

        public Vector2? node;

        public bool goBack;

        public Vector2 anchor;

        public SineWave sine;

        public float respawnTimer;

        public bool fireMode;

        public Wiggler hitWiggler;

        public Vector2 hitDir;

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
                    bumpersprite.Play("on");
                    bumperspriteEvil.Play("on");
                    if (!fireMode)
                    {
                        Audio.Play("event:/game/06_reflection/pinballbumper_reset", Position);
                    }
                }
            }
            else if (base.Scene.OnInterval(0.05f))
            {
                float num = Calc.Random.NextAngle();
                ParticleType type = (fireMode ? Bumper.P_FireAmbience : Bumper.P_Ambience);
                float direction = (fireMode ? (-MathF.PI / 2f) : num);
                float length = (fireMode ? 12 : 8);
                SceneAs<Level>().Particles.Emit(type, 1, base.Center + Calc.AngleToVector(num, length), Vector2.One * 2f, direction);
            }

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            fireMode = SceneAs<Level>().CoreMode == Session.CoreModes.Hot;
            bumperspriteEvil.Visible = fireMode;
            bumpersprite.Visible = !fireMode;
        }

        public void OnBumperChangeMode(Session.CoreModes coreMode)
        {
            fireMode = coreMode == Session.CoreModes.Hot;
            bumperspriteEvil.Visible = fireMode;
            bumpersprite.Visible = !fireMode;
        }

    }
}