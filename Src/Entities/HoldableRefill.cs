using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Collections;

namespace Celeste.Mod.ReverseHelper.Entities
{
    //internal class HoldableRefill

    // Token: 0x02000350 RID: 848
    [CustomEntity("ReverseHelper/HoldableRefill")]
    public class HoldableRefill : Actor
    {
        private float refillTime = 2.5f;
        private Player? player;
        private bool playerCollide = false;
        private bool hold_grace = false;
        [SourceGen.Loader.Load]
        public static void Load()
        {
            On.Celeste.Player.NormalUpdate += normalupd;
        }
        [SourceGen.Loader.Unload]

        public static void Unload()
        {
            On.Celeste.Player.NormalUpdate -= normalupd;
        }

        private static int normalupd(On.Celeste.Player.orig_NormalUpdate orig, Player self)
        {
            int result = orig(self);
            if (self.Holding?.Entity is HoldableRefill href)
            {
                bool flag = self.CanDash && href.dashable;
                if (flag)
                {
                    //self.Drop();
                    return self.StartDash();
                }


            }
            return result;
        }
        bool throwRefill;
        bool throwRefill2;
        bool grabbingRefill;
        private bool refillOnHolding;

        // Token: 0x06001A93 RID: 6803 RVA: 0x000AB8EC File Offset: 0x000A9AEC
        public HoldableRefill(Vector2 position, bool twoDashes, bool oneUse, bool refillOnHolding, bool floating, bool slowFall, bool stillRefillOnNoDash, bool dashable, float refilltime, bool throwRefill, bool throwRefill2, bool grabbingRefill) : base(position)
        {
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider((x) =>
            {
                playerCollide = true;
                player = x;
            }, null, null));
            this.twoDashes = twoDashes;
            this.oneUse = oneUse;
            string str;
            if (twoDashes)
            {
                str = "objects/refillTwo/";
                p_shatter = P_ShatterTwo;
                p_regen = P_RegenTwo;
                p_glow = P_GlowTwo;
            }
            else
            {
                str = "objects/refill/";
                p_shatter = P_Shatter;
                p_regen = P_Regen;
                p_glow = P_Glow;
            }
            Add(outline = new Image(GFX.Game[str + "outline"]));
            outline.CenterOrigin();
            outline.Visible = false;
            Add(sprite = new Sprite(GFX.Game, str + "idle"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle", false, false);
            sprite.CenterOrigin();
            Add(flash = new Sprite(GFX.Game, str + "flash"));
            flash.Add("flash", "", 0.05f);
            flash.OnFinish = delegate (string anim)
            {
                flash.Visible = false;
            };
            flash.CenterOrigin();
            Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v)
                {
                    sprite.Scale = flash.Scale = Vector2.One * (1f + v * 0.2f);
                }, false, false));
            Add(new MirrorReflection());
            Add(bloom = new BloomPoint(0.8f, 16f));
            Add(light = new VertexLight(Color.White, 1f, 16, 48));
            Add(sine = new SineWave(0.6f, 0f));
            sine.Randomize();
            UpdateY();
            Depth = -100;

            //
            Add(Hold = new Holdable(0.1f));
            Hold.PickupCollider = new Hitbox(24f, 24f, -12f, -12f);
            Hold.SlowFall = false;
            Hold.SlowRun = true;
            Hold.OnPickup = pickup;
            Hold.OnRelease = release;
            //this.Hold.DangerousCheck = new Func<HoldableCollider, bool>(this.Dangerous);
            //this.Hold.OnHitSeeker = new Action<Seeker>(this.HitSeeker);
            //this.Hold.OnSwat = new Action<HoldableCollider, int>(this.Swat);
            Hold.OnHitSpring = (spring) =>
            {
                if (!Hold.IsHeld)
                {
                    if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
                    {
                        Speed.X = Speed.X * 0.5f;
                        Speed.Y = -160f;
                        noGravityTimer = 0.15f;
                        return true;
                    }
                    if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
                    {
                        MoveTowardsY(spring.CenterY + 5f, 4f, null);
                        Speed.X = slowFall ? 160f : 220f;
                        Speed.Y = -80f;
                        noGravityTimer = 0.1f;
                        return true;
                    }
                    if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
                    {
                        MoveTowardsY(spring.CenterY + 5f, 4f, null);
                        Speed.X = slowFall ? -160f : -220f;
                        Speed.Y = -80f;
                        noGravityTimer = 0.1f;
                        return true;
                    }
                }
                return false;
            };
            //this.Hold.OnHitSpinner = new Action<Entity>(this.HitSpinner);
            Hold.SpeedGetter = () => Speed;
            onCollideH = (data) =>
            {
                (data.Hit as DashSwitch)?.OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
                //Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
                //if (Math.Abs(Speed.X) > 100f)
                //{
                //    //this.ImpactParticles(data.Direction);
                //}
                Speed.X = Speed.X * (slowFall ? -1f : -0.4f);
            };
            onCollideV = data =>
            {
                (data.Hit as DashSwitch)?.OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
                if (Speed.Y > 0f)
                {
                    //if (hardVerticalHitSoundCooldown <= 0f)
                    //{
                    //    //Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0f, 200f, 0f, 1f));
                    //    //hardVerticalHitSoundCooldown = 0.5f;
                    //}
                    //else
                    //{
                    //    //Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", 0f);
                    //}
                }
                if (Speed.Y > 160f)
                {
                    //this.ImpactParticles(data.Direction);
                }
                if (slowFall)
                {
                    if (Speed.Y < 0f)
                    {
                        Speed.Y = Speed.Y * -0.5f;
                        return;
                    }
                }
                else if (Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
                {
                    Speed.Y = Speed.Y * -0.6f;
                    return;
                }
                Speed.Y = 0f;
            };
            LiftSpeedGraceTime = 0.1f;
            Add(new VertexLight(Collider.Center, Color.White, 1f, 32, 64));
            //Tag = Tags.TransitionUpdate;
            this.refillOnHolding = refillOnHolding;
            this.floating = floating;
            this.slowFall = slowFall;
            Hold.SlowFall = slowFall;
            this.stillRefillOnNoDash = stillRefillOnNoDash;
            this.dashable = dashable;
            this.refillTime = refilltime;
            //base.Add(new MirrorReflection());
            SquishCallback = OnSquish;
            this.throwRefill = throwRefill;
            this.throwRefill2 = throwRefill2;
            this.grabbingRefill = grabbingRefill;
        }

        public override void OnSquish(CollisionData data)
        {
            if (!TrySquishWiggle(data, 8, 8))
            {
                RemoveSelf();
            }
        }

        // Token: 0x06001A94 RID: 6804 RVA: 0x000ABB4C File Offset: 0x000A9D4C
        public HoldableRefill(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("twoDash", false),
            data.Bool("oneUse", false), data.Bool("refillOnHolding", true), data.Bool("floaty", false), data.Bool("slowFall", false),
            data.Bool("stillRefillOnNoDash", false), data.Bool("dashable", false), data.Float("refilltime", 2.5f),
            data.Bool("refillOnThrow", false), data.Bool("refillOnThrow2", false), data.Bool("refillWhenGrabbing", false))
        {
        }
        const float MaxThrowTimer = 0.06f;
        float ThrowTimer = MaxThrowTimer;
        // Token: 0x06001A96 RID: 6806 RVA: 0x000ABB90 File Offset: 0x000A9D90
        public override void Update()
        {
            base.Update();
            if (swatTimer > 0f)
            {
                swatTimer -= Engine.DeltaTime;
            }
            //hardVerticalHitSoundCooldown -= Engine.DeltaTime;
            if (OnPedestal)
            {
                Depth = 8999;
                return;
            }
            Depth = 100;
            if (Hold.Holder != null)
            {
                if (Hold.Holder.StateMachine != Player.StPickup || !grabbingRefill)
                {
                    hold_grace = true;
                }
                ThrowTimer = MaxThrowTimer;
            }
            else
            {
                if (throwRefill)
                {
                    hold_grace = false;
                }
                else if (throwRefill2)
                {
                    if (ThrowTimer <= 0 || !playerCollide)
                    {
                        hold_grace = false;
                    }
                    else
                    {
                        ThrowTimer -= Engine.DeltaTime;
                    }
                }
                else
                {
                    if (!playerCollide)
                    {
                        hold_grace = false;
                    }
                }
            }


            if (playerCollide)
            {
                OnPlayer_should_later_than_holdcheck(player!);
            }

            playerCollide = false;
            using (List<Entity>.Enumerator enumerator = Scene.Tracker.GetEntities<SeekerBarrier>().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Entity entity = enumerator.Current;
                    SeekerBarrier seekerBarrier = (SeekerBarrier)entity;
                    seekerBarrier.Collidable = true;
                    bool flag = CollideCheck(seekerBarrier);
                    seekerBarrier.Collidable = false;
                    if (flag)
                    {
                        //this.destroyed = true;
                        Collidable = false;
                        if (Hold.IsHeld)
                        {
                            Vector2 speed2 = Hold.Holder!.Speed;
                            Hold.Holder.Drop();
                            Speed = speed2 * 0.333f;
                            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        }
                        Audio.Play("event:/new_content/game/10_farewell/glider_emancipate", Position);
                        RemoveSelf();
                        //base.Add(new Coroutine(this.DestroyAnimationRoutine(), true));
                        return;
                    }
                }
            }
            if (Hold.IsHeld)
            {
                prevLiftSpeed = Vector2.Zero;
            }
            else
            {
                if (!floating)
                {
                    if (highFrictionTimer > 0f)
                    {
                        highFrictionTimer -= Engine.DeltaTime;
                    }
                    if (OnGround(1))
                    {
                        float target;
                        if (!OnGround(Position + Vector2.UnitX * 3f, 1))
                        {
                            target = 20f;
                        }
                        else if (!OnGround(Position - Vector2.UnitX * 3f, 1))
                        {
                            target = -20f;
                        }
                        else
                        {
                            target = 0f;
                        }
                        Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime);
                        Vector2 liftSpeed = LiftSpeed;
                        if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
                        {
                            Speed = prevLiftSpeed;
                            prevLiftSpeed = Vector2.Zero;
                            Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                            if (Speed.X != 0f && Speed.Y == 0f)
                            {
                                Speed.Y = -60f;
                            }
                            if (Speed.Y < 0f)
                            {
                                noGravityTimer = 0.15f;
                            }
                        }
                        else
                        {
                            prevLiftSpeed = liftSpeed;
                            if (liftSpeed.Y < 0f && Speed.Y < 0f)
                            {
                                Speed.Y = 0f;
                            }
                        }
                    }
                    else if (Hold.ShouldHaveGravity)
                    {
                        float num = slowFall ? 200f : 800f;
                        if (Math.Abs(Speed.Y) <= 30f)
                        {
                            num *= 0.5f;
                        }
                        float num2 = 350f;
                        //if (Speed.Y < 0f)
                        //{
                        //    num2 *= 0.5f;
                        //}
                        if (Speed.Y < 0f)
                        {
                            num2 = slowFall ? 40 : 350f * 0.5f;
                        }
                        else if (highFrictionTimer <= 0f && slowFall)
                        {
                            num2 = 40f;
                        }
                        else
                        {
                            num2 = slowFall ? 10f : 350f;
                        }

                        Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
                        if (noGravityTimer > 0f)
                        {
                            noGravityTimer -= Engine.DeltaTime;
                        }
                        else if (Level.Wind.Y < 0f && slowFall)
                        {
                            Speed.Y = Calc.Approach(Speed.Y, 0f, num * Engine.DeltaTime);
                        }
                        else
                        {
                            Speed.Y = Calc.Approach(Speed.Y, slowFall ? 30f : 200f, num * Engine.DeltaTime);
                        }
                    }
                    previousPosition = ExactPosition;
                    MoveH(Speed.X * Engine.DeltaTime, onCollideH, null);
                    MoveV(Speed.Y * Engine.DeltaTime, onCollideV, null);

                    if (Left < Level.Bounds.Left)
                    {
                        Left = Level.Bounds.Left;
                        onCollideH(new CollisionData
                        {
                            Direction = -Vector2.UnitX
                        });
                    }
                    else if (Right > Level.Bounds.Right)
                    {
                        Right = Level.Bounds.Right;
                        onCollideH(new CollisionData
                        {
                            Direction = Vector2.UnitX
                        });
                    }
                    if (Top < Level.Bounds.Top)
                    {
                        Top = Level.Bounds.Top;
                        onCollideV(new CollisionData
                        {
                            Direction = -Vector2.UnitY
                        });
                    }
                    else if (Top > Level.Bounds.Bottom + 16)
                    {
                        RemoveSelf();
                        return;
                    }
                    //if (Center.X > Level.Bounds.Right)
                    //{
                    //    MoveH(32f * Engine.DeltaTime, null, null);
                    //    if (Left - 8f > Level.Bounds.Right)
                    //    {
                    //        //RemoveSelf();
                    //    }
                    //}
                    //else if (Left < Level.Bounds.Left)
                    //{
                    //    Left = Level.Bounds.Left;
                    //    Speed.X = Speed.X * -0.4f;
                    //}
                    //else if (Top < Level.Bounds.Top - 4)
                    //{
                    //    Top = Level.Bounds.Top + 4;
                    //    Speed.Y = 0f;
                    //}
                    //else if (base.Bottom > (float)this.Level.Bounds.Bottom && SaveData.Instance.Assists.Invincible)
                    //{
                    //    base.Bottom = (float)this.Level.Bounds.Bottom;
                    //    this.Speed.Y = -300f;
                    //    Audio.Play("event:/game/general/assist_screenbottom", this.Position);
                    //}
                    //else if (base.Top > (float)this.Level.Bounds.Bottom)
                    //{
                    //    this.Die();
                    //}
                    //if (X < Level.Bounds.Left + 10)
                    //{
                    //    MoveH(32f * Engine.DeltaTime, null, null);
                    //}
                    //Player entity = Scene.Tracker.GetEntity<Player>();
                    //TempleGate templeGate = CollideFirst<TempleGate>();
                    //if (templeGate != null && entity != null)
                    //{
                    //    templeGate.Collidable = false;
                    //    MoveH(Math.Sign(entity.X - X) * 32 * Engine.DeltaTime, null, null);
                    //    templeGate.Collidable = true;
                    //}
                    Hold.CheckAgainstColliders();
                }
                else
                {
                    //this.Position = this.startPos + Vector2.UnitY * this.platformSine.Value * 1f;
                }
            }
            //
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
                Level.ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
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

        private bool refilled = true;

        // Token: 0x06001A97 RID: 6807 RVA: 0x000ABCB4 File Offset: 0x000A9EB4
        private void Respawn()
        {
            if (!refilled)
            {
                refilled = true;
                Collidable = true;
                sprite.Visible = true;
                outline.Visible = false;
                Depth = -100;
                wiggler.Start();
                Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_return" : "event:/game/general/diamond_return", Position);
                Level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
            }
        }

        // Token: 0x06001A98 RID: 6808 RVA: 0x000ABD48 File Offset: 0x000A9F48
        private void UpdateY()
        {
            flash.Y = sprite.Y = bloom.Y = sine.Value * 2f;
        }

        // Token: 0x06001A99 RID: 6809 RVA: 0x000ABD8D File Offset: 0x000A9F8D
        public override void Render()
        {
            if (sprite.Visible)
            {
                sprite.DrawOutline(1);
            }
            base.Render();
        }

        // Token: 0x06001A9A RID: 6810 RVA: 0x000ABDB0 File Offset: 0x000A9FB0
        private void OnPlayer_should_later_than_holdcheck(Player player)
        {
            if (refilled && ((refillOnHolding || !/*Hold.IsHeld*/hold_grace) || (player.Dashes == 0 && player.MaxDashes != 0 && stillRefillOnNoDash)))
            {
                if (!player.Dead && player.UseRefill(twoDashes))
                {
                    Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", Position);
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    refilled = false;
                    Add(new Coroutine(RefillRoutine(player), true));
                    respawnTimer = refillTime;
                }
            }
        }

        // Token: 0x06001A9B RID: 6811 RVA: 0x000ABE17 File Offset: 0x000AA017
        private IEnumerator RefillRoutine(Player player)
        {
            Celeste.Freeze(0.05f);
            yield return null;
            Level.Shake(0.3f);
            sprite.Visible = flash.Visible = false;
            if (!oneUse)
            {
                outline.Visible = true;
            }
            Depth = 8999;
            yield return 0.05f;
            float num = player.Speed.Angle();
            Level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - 1.57079637f);
            Level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + 1.57079637f);
            SlashFx.Burst(Position, num);
            if (oneUse)
            {
                RemoveSelf();
            }
            yield break;
        }

        private void pickup()
        {
            //refill is wider than player, so it is in the wall when player throws it against the wall,
            //and then it will move to the bottom of the wall.
            Collider.Position.X = -4f;
            Collider.Width = 8f;

            hold_grace = true;
            highFrictionTimer = 0.5f;
            floating = false;
            //Hold.IsHeld = true;
            Speed = Vector2.Zero;
            AddTag(Tags.Persistent);
        }

        private void release(Vector2 force)
        {
            Collider.Position.X = -8f;
            Collider.Width = 16f;

            //if (CollideCheck<Solid>())
            //{
            for (int i = 0; i <= 8; i++)
            {
                for (int j = 0; j <= 8; j++)
                {
                    for (int k = 1; k >= -1; k -= 2)
                    {
                        for (int l = 1; l >= -1; l -= 2)
                        {
                            if (!CollideCheck<Solid>(Position + new Vector2(i * k, j * l)))
                            {
                                Position += new Vector2(i * k, j * l);
                                goto IL_114514;
                            }
                        }
                    }
                }
            }
        IL_114514:
            //force = Vector2.Zero;//why?
            //}
            //Hold.IsHeld = false;
            RemoveTag(Tags.Persistent);
            if (slowFall)
            {
                force.Y *= 0.5f;
            }
            if (force.X != 0f && force.Y == 0f)
            {
                force.Y = -0.4f;
            }
            Speed = force * (slowFall ? 100f : 200f);
            if (Speed != Vector2.Zero)
            {
                noGravityTimer = 0.1f;
            }
        }

        // Token: 0x04001731 RID: 5937
        public static ParticleType P_Shatter { get => Refill.P_Shatter; }

        // Token: 0x04001732 RID: 5938
        public static ParticleType P_Regen { get => Refill.P_Regen; }

        // Token: 0x04001733 RID: 5939
        public static ParticleType P_Glow { get => Refill.P_Glow; }

        // Token: 0x04001734 RID: 5940
        public static ParticleType P_ShatterTwo { get => Refill.P_ShatterTwo; }

        // Token: 0x04001735 RID: 5941
        public static ParticleType P_RegenTwo { get => Refill.P_RegenTwo; }

        // Token: 0x04001736 RID: 5942
        public static ParticleType P_GlowTwo { get => Refill.P_GlowTwo; }

        // Token: 0x04001737 RID: 5943
        private Sprite sprite;

        // Token: 0x04001738 RID: 5944
        private Sprite flash;

        // Token: 0x04001739 RID: 5945
        private Image outline;

        // Token: 0x0400173A RID: 5946
        private Wiggler wiggler;

        // Token: 0x0400173B RID: 5947
        private BloomPoint bloom;

        // Token: 0x0400173C RID: 5948
        private VertexLight light;

        // Token: 0x0400173D RID: 5949

        // Token: 0x0400173E RID: 5950
        private SineWave sine;

        // Token: 0x0400173F RID: 5951
        private bool twoDashes;

        // Token: 0x04001740 RID: 5952
        private bool oneUse;

        // Token: 0x04001741 RID: 5953
        private ParticleType p_shatter;

        // Token: 0x04001742 RID: 5954
        private ParticleType p_regen;

        // Token: 0x04001743 RID: 5955
        private ParticleType p_glow;

        // Token: 0x04001744 RID: 5956
        private float respawnTimer;

        //public static ParticleType P_Impact;

        // Token: 0x040017B1 RID: 6065

        public Vector2 Speed;

        // Token: 0x040017B2 RID: 6066
        public bool OnPedestal;

        // Token: 0x040017B3 RID: 6067
        public Holdable Hold;

        // Token: 0x040017B6 RID: 6070
        private Level Level => SceneAs<Level>();

        // Token: 0x040017B7 RID: 6071
        private Collision onCollideH;

        // Token: 0x040017B8 RID: 6072
        private Collision onCollideV;

        // Token: 0x040017B9 RID: 6073
        private float noGravityTimer;

        // Token: 0x040017BA RID: 6074
        private Vector2 prevLiftSpeed;

        // Token: 0x040017BB RID: 6075
        private Vector2 previousPosition;

        // Token: 0x040017BD RID: 6077
        private float swatTimer;

        // Token: 0x040017BF RID: 6079
        //private float hardVerticalHitSoundCooldown;

        // Token: 0x040017C0 RID: 6080
        //private BirdTutorialGui tutorialGui;

        // Token: 0x040017C1 RID: 6081
        //private float tutorialTimer;

        private bool floating;
        private bool slowFall;
        private float highFrictionTimer;

        //private bool destroyed;
        private bool stillRefillOnNoDash;

        private bool dashable;
    }
}