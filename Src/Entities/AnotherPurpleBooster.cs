using Celeste.Mod.Entities;
using Celeste.Mod.Meta;
using Celeste.Mod.ReverseHelper._From_Vortex_Helper;
using Celeste.Mod.ReverseHelper.SourceGen.Loader;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Reflection;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [CustomEntity("ReverseHelper/AnotherPurpleBooster")]
    [Tracked]
    public class AnotherPurpleBooster : Entity
    {
        internal const string POSSIBLE_EARLY_DASHSPEED = "purpleBoostPossibleEarlyDashSpeed";

        private Sprite? sprite;
        private Wiggler wiggler;
        private Entity outline;

        private MTexture linkSegCenter, linkSegCenterOutline, linkSeg, linkSegOutline;

        private Coroutine dashRoutine;
        private DashListener dashListener;

        private float respawnTimer;
        private float cannotUseTimer;

        public bool BoostingPlayer
        {
            get;
            set;
        }

        public bool StartedBoosting;

        private bool linkVisible = false;
        private float actualLinkPercent = 1.0f;
        private float linkPercent = 1.0f;

        public static ParticleType P_Burst = new ParticleType(Booster.P_Burst);
        public static ParticleType P_Appear = new ParticleType(Booster.P_Appear);
        public static ParticleType P_BurstExplode = new ParticleType(Booster.P_Burst);
        static AnotherPurpleBooster()
        {
            InitializeParticles();
        }

        private SoundSource loopingSfx;
        //public static bool AnotherRedirect = true;
        public bool redirect = true;

        //public static bool DashAtt = true;
        public bool dashatt = false;
        public const float usingDashAtt = 0.1f;

        //public static bool FixSomething1 = true;
        public bool fixsomething1 = false;
        public bool conserveSpeed;
        public bool conserveSpeedV;
        public float conserveMovingSpeedCutOff;
        public bool neverSlowDown;//not used. never slow down now for sure.
        public AnotherPurpleBooster(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("redirect", true), data.Bool("addDashAttack", false), data.Bool("NewAction", false),
                  data.Bool("conserveSpeed", false), data.Bool("conserveSpeedV", true), data.Float("conserveMovingSpeedCutOff", 100000), /*data.Bool("neverSlowDown", false)*/true)
        {
        }

        public AnotherPurpleBooster(Vector2 position, bool redirected, bool dashattack, bool fix1,
            bool conserveSpeed, bool conserveSpeedV, float conserveMovingSpeed, bool neverSlowDown)
            : base(position)
        {

            fixsomething1 = fix1;
            dashatt = dashattack;
            redirect = redirected;

            Depth = Depths.Above;
            Collider = new Circle(10f, 0f, 2f);

            if ((sprite = ReverseHelperExtern.VortexHelperModule.PurpleBoosterSpriteBank?.Create("purpleBooster")) is not null)
            {
                Add(sprite);
            }

            Add(new PlayerCollider(OnPlayer));
            Add(new VertexLight(Color.White, 1f, 16, 32));
            Add(new BloomPoint(0.1f, 16f));
            Add(wiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                if (sprite is not null)
                {
                    sprite.Scale = Vector2.One * (1f + f * 0.25f);
                }
            }));

            linkSegCenter = GFX.Game["objects/VortexHelper/slingBooster/link03"];
            linkSegCenterOutline = GFX.Game["objects/VortexHelper/slingBooster/link02"];
            linkSeg = GFX.Game["objects/VortexHelper/slingBooster/link01"];
            linkSegOutline = GFX.Game["objects/VortexHelper/slingBooster/link00"];

            Add(dashRoutine = new Coroutine(removeOnComplete: false));
            Add(dashListener = new DashListener());

            Add(new MirrorReflection());
            Add(loopingSfx = new SoundSource());

            dashListener.OnDash = OnPlayerDashed;
            this.conserveSpeed = conserveSpeed;
            this.conserveSpeedV = conserveSpeedV;
            this.conserveMovingSpeedCutOff = conserveMovingSpeed;
            if (conserveMovingSpeedCutOff < 0)
            {
                conserveMovingSpeedCutOff = 0;
            }
            this.neverSlowDown = neverSlowDown;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (sprite is null)
            {
                RemoveSelf();
                return;
            }

            Image image = new Image(GFX.Game["objects/booster/outline"]);
            image.CenterOrigin();
            image.Color = Color.White * 0.75f;
            outline = new Entity(Position)
            {
                Depth = Depths.BGDecals - 1,
                Visible = false
            };
            outline.Y += 2f;
            outline.Add(image);
            outline.Add(new MirrorReflection());
            scene.Add(outline);
        }

        private void AppearParticles()
        {
            ParticleSystem particlesBG = SceneAs<Level>().ParticlesBG;
            for (int i = 0; i < 360; i += 30)
            {
                particlesBG.Emit(P_Appear, 1, Center, Vector2.One * 2f, i * ((float)Math.PI / 180f));
            }
        }

        private void OnPlayer(Player player)
        {
            if (respawnTimer <= 0f && cannotUseTimer <= 0f && !BoostingPlayer)
            {
                linkPercent = actualLinkPercent = 1.0f;
                linkVisible = false;

                cannotUseTimer = 0.45f;

                Boost(player, this);

                Audio.Play(SFX.game_04_greenbooster_enter, Position);
                wiggler.Start();
                sprite.Play("inside", false, false);
            }
        }

        public static void Boost(Player player, AnotherPurpleBooster booster)
        {
            //todo
            //AnotherRedirect = booster.redirect;
            //DashAtt = booster.dashatt;
            //FixSomething1 = booster.fixsomething1;
            DynData<Player> playerData = player.GetData();
            playerData.Set("USSRNAME_ConserveSpeedSave", player.Speed);

            player.StateMachine.State = ReverseHelperModule.AnotherPurpleBoosterState;
            player.Speed = Vector2.Zero;
            playerData.Set("boostTarget", booster.Center);
            booster.StartedBoosting = true;

            playerData.Set("USSRNAME_AnotherRedirect", booster.redirect);
            playerData.Set("USSRNAME_DashAtt", booster.dashatt);
            playerData.Set("USSRNAME_FixSomething1", booster.fixsomething1);
            playerData.Set("USSRNAME_ConserveSpeed", booster.conserveSpeed);
            playerData.Set("USSRNAME_ConserveSpeedV", booster.conserveSpeedV);
            playerData.Set("USSRNAME_NeverSlowDown", booster.neverSlowDown);
            playerData.Set("USSRNAME_ConserveMovingSpeedCutOff", booster.conserveMovingSpeedCutOff);
        }

        public void PlayerBoosted(Player player, Vector2 direction)
        {
            StartedBoosting = false;
            BoostingPlayer = false;
            linkVisible = true;
            Audio.Play(SFX.game_04_greenbooster_dash, Position);
            loopingSfx.Play(SFX.game_05_redbooster_move_loop);

            loopingSfx.DisposeOnTransition = false;

            BoostingPlayer = true;
            Tag = Tags.Persistent | Tags.TransitionUpdate;
            sprite.Play("spin", false, false);
            wiggler.Start();
            dashRoutine.Replace(BoostRoutine(player, direction));
        }

        private IEnumerator BoostRoutine(Player player, Vector2 dir)
        {
            Level level = SceneAs<Level>();
            while (player.StateMachine.State == ReverseHelperModule.AnotherPurpleBoosterDashState && BoostingPlayer)
            {
                if (player.Dead)
                {
                    PlayerDied();
                }
                else
                {
                    sprite.RenderPosition = player.Center;
                    loopingSfx.Position = sprite.Position;
                    if (Scene.OnInterval(0.02f))
                    {
                        level.ParticlesBG.Emit(P_Burst, 2, player.Center - dir * 3f + new Vector2(0f, -2f), new Vector2(3f, 3f));
                    }
                    yield return null;
                }
            }
            PlayerReleased();
            if (player.StateMachine.State == Player.StBoost)
            {
                sprite.Visible = false;
            }
            linkVisible = player.StateMachine.State == Player.StDash || player.StateMachine.State == Player.StNormal;
            linkPercent = linkVisible ? 0.0f : 1.0f;

            if (!linkVisible)
            {
                LaunchPlayerParticles(player, -dir, P_BurstExplode);
            }

            while (SceneAs<Level>().Transitioning)
            {
                yield return null;
            }
            Tag = 0;
            yield break;
        }

        private void OnPlayerDashed(Vector2 direction)
        {
            if (BoostingPlayer)
            {
                BoostingPlayer = false;
            }
        }

        private void PlayerReleased()
        {
            Audio.Play(SFX.game_05_redbooster_end, sprite.RenderPosition);
            sprite.Play("pop");
            cannotUseTimer = 0f;
            respawnTimer = 1f;
            BoostingPlayer = false;
            outline.Visible = true;
            loopingSfx.Stop();
        }

        private void PlayerDied()
        {
            if (BoostingPlayer)
            {
                PlayerReleased();
                dashRoutine.Active = false;
                Tag = 0;
            }
        }

        private void Respawn()
        {
            Audio.Play(SFX.game_04_greenbooster_reappear, Position);
            sprite.Position = Vector2.Zero;
            sprite.Play("appear", restart: true);
            sprite.Visible = true;
            outline.Visible = false;
            AppearParticles();
        }

        public override void Update()
        {
            base.Update();

            actualLinkPercent = Calc.Approach(actualLinkPercent, linkPercent, 5f * Engine.DeltaTime);

            if (cannotUseTimer > 0f)
            {
                cannotUseTimer -= Engine.DeltaTime;
            }
            if (respawnTimer > 0f)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f)
                {
                    Respawn();
                }
            }
            if (!dashRoutine.Active && respawnTimer <= 0f)
            {
                Vector2 target = Vector2.Zero;
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null && CollideCheck(entity))
                {
                    target = entity.Center + Booster.playerOffset - Position;
                }
                sprite.Position = Calc.Approach(sprite.Position, target, 80f * Engine.DeltaTime);
            }
            if (sprite.CurrentAnimationID == "inside" && !BoostingPlayer && !CollideCheck<Player>())
            {
                sprite.Play("loop");
            }
        }

        public static void LaunchPlayerParticles(Player player, Vector2 dir, ParticleType p)
        {
            Level level = player.SceneAs<Level>();
            float angle = dir.Angle() - 0.5f;
            for (int i = 0; i < 20; i++)
            {
                level.ParticlesBG.Emit(p, 1, player.Center, new Vector2(3f, 3f), angle + Calc.Random.NextFloat());
            }
        }

        public override void Render()
        {
            Vector2 position = sprite.Position;
            sprite.Position = position.Floor();

            if (sprite.CurrentAnimationID != "pop" && sprite.Visible)
            {
                sprite.DrawOutline();
            }

            if (linkVisible)
            {
                RenderPurpleBoosterLink(12, 0.35f);
            }

            base.Render();
            sprite.Position = position;
        }

        private void RenderPurpleBoosterLink(int spriteCount, float minScale)
        {
            float increment = 1f / spriteCount;
            float centerSegmentScale = 0.7f + 0.3f * actualLinkPercent;
            linkSegCenterOutline.DrawOutlineCentered(Center, Color.Black, centerSegmentScale);
            for (float t = increment; t <= actualLinkPercent; t += increment) // Black Outline
            {
                Vector2 vec = Vector2.Lerp(Center, sprite.RenderPosition, t * actualLinkPercent);
                linkSegOutline.DrawOutlineCentered(vec, Color.Black, 1.01f - (t * minScale));
            }
            linkSegCenterOutline.DrawCentered(Center, Color.White, centerSegmentScale);
            for (float t = increment; t <= actualLinkPercent; t += increment) // Pink Outline
            {
                Vector2 vec = Vector2.Lerp(Center, sprite.RenderPosition, t * actualLinkPercent);
                linkSegOutline.DrawCentered(vec, Color.White, 1.01f - (t * minScale));
            }

            linkSegCenter.DrawCentered(Center, Color.White, centerSegmentScale); // Sprites
            for (float t = increment; t <= actualLinkPercent; t += increment)
            {
                Vector2 vec = Vector2.Lerp(Center, sprite.RenderPosition, t * actualLinkPercent);
                linkSeg.DrawCentered(vec, Color.White, 1f - (t * minScale));
            }
        }

        public static void InitializeParticles()
        {
            P_Burst.Color = Calc.HexToColor("8c2c95");
            P_Appear.Color = Calc.HexToColor("b64acf");

            P_BurstExplode.Color = P_Burst.Color;
            P_BurstExplode.SpeedMax = 250; // felt like good value
        }

        #region Custom Purple Booster Behavior

        // TODO: Merge the two states into one. Don't know why I separated them...

        // Inside the Purple Booster
        public static void PurpleBoostBegin()
        {
            Util.TryGetPlayer(out Player player);
            player.CurrentBooster = null;

            // Fixes hair sticking out of the bubble sprite when entering it ducking.
            // If for whatever reason this breaks an older map, this will be removed.
            player.Ducking = false;

            Level level = player.SceneAs<Level>();
            bool? flag;
            if (level == null)
            {
                flag = null;
            }
            else
            {
                MapMetaModeProperties meta = level.Session.MapData.GetMeta();
                flag = meta?.TheoInBubble;
            }
            bool? flag2 = flag;
            player.RefillDash();
            player.RefillStamina();
            if (flag2.GetValueOrDefault())
            {
                return;
            }
            player.Drop();
        }

        public static int PurpleBoostUpdate()
        {
            Util.TryGetPlayer(out Player player);
            DynData<Player> playerData = player.GetData();

            Vector2 boostTarget = playerData.Get<Vector2>("boostTarget");
            Vector2 value = Input.Aim.Value * 3f;
            Vector2 vector = Calc.Approach(player.ExactPosition, boostTarget - player.Collider.Center + value, 80f * Engine.DeltaTime);

            player.MoveToX(vector.X, null);
            player.MoveToY(vector.Y, null);

            if (Vector2.DistanceSquared(player.Center, boostTarget) >= 275f)
            {
                foreach (AnotherPurpleBooster b in player.Scene.Tracker.GetEntities<AnotherPurpleBooster>())
                {
                    if (b.StartedBoosting)
                    {
                        b.PlayerReleased();
                    }
                }
                return 0;
            }

            // now supports demobutton presses
            if (Input.DashPressed || Input.CrouchDashPressed)
            {
                // we don't need to do this, we're not actually dashing here- but fastbubbling.
                //demoDashed = Input.CrouchDashPressed;
                Input.Dash.ConsumePress();
                Input.CrouchDash.ConsumeBuffer();
                return ReverseHelperModule.AnotherPurpleBoosterDashState;
            }

            return ReverseHelperModule.AnotherPurpleBoosterState;
        }

        public static void PurpleBoostEnd()
        {
            Util.TryGetPlayer(out Player player);
            Vector2 boostTarget = player.GetData().Get<Vector2>("boostTarget");
            Vector2 vector = (boostTarget - player.Collider.Center).Floor();

            player.MoveToX(vector.X, null);
            player.MoveToY(vector.Y, null);
        }

        public static IEnumerator PurpleBoostCoroutine()
        {
            yield return 0.3f;

            Util.TryGetPlayer(out Player player);
            player.StateMachine.State = ReverseHelperModule.AnotherPurpleBoosterDashState;
        }

        // Arc Motion
        public static void PurpleDashingBegin()
        {
            Celeste.Freeze(0.05f); // this freeze makes fastbubbling much more lenient

            Util.TryGetPlayer(out Player player);
            DynData<Player> playerData = player.GetData();
            player.DashDir = Input.GetAimVector(player.Facing);
            playerData.Set(POSSIBLE_EARLY_DASHSPEED, Vector2.Zero);

            foreach (AnotherPurpleBooster b in player.Scene.Tracker.GetEntities<AnotherPurpleBooster>())
            {
                if (b.StartedBoosting)
                {
                    b.PlayerBoosted(player, player.DashDir);
                    return;
                }
                if (b.BoostingPlayer)
                {
                    return;
                }
            }
        }

        public static int PurpleDashingUpdate()
        {
            addDashAttack();
            if (Input.DashPressed || Input.CrouchDashPressed)
            {
                Util.TryGetPlayer(out Player player);
                DynData<Player> playerData = player.GetData();

                player.LiftSpeed += playerData.Get<Vector2>(POSSIBLE_EARLY_DASHSPEED);

                return player.StartDash();
            }

            return ReverseHelperModule.AnotherPurpleBoosterDashState;
        }

        public static IEnumerator PurpleDashingCoroutine()
        {
            float t = 0f;
            Util.TryGetPlayer(out Player player);
            DynData<Player> playerData = player.GetData();
            Vector2 origin = playerData.Get<Vector2>("boostTarget");
            player.Speed = playerData.Get<Vector2>("USSRNAME_ConserveSpeedSave");
            //player.MoveToX(origin.X);
            //player.MoveToY(origin.Y + 6f);
            Vector2 earlyExitBoost;

            float mov = 0;
            Vector2 dir = player.DashDir;
            bool AnotherRedirect = playerData.Get<bool>("USSRNAME_AnotherRedirect");
            float conserveCutOff = playerData.Get<float>("USSRNAME_ConserveMovingSpeedCutOff");
            bool conserveSpeed = playerData.Get<bool>("USSRNAME_ConserveSpeed");
            bool conserveSpeedV = playerData.Get<bool>("USSRNAME_ConserveSpeedV");
            bool neverSlowDown = playerData.Get<bool>("USSRNAME_NeverSlowDown");


            const double speedconstant = 1.5 * Math.PI * 60;
            double rate = 1;
            float lastfspeed = (float)(1 * speedconstant);
            if (conserveSpeed)
            {
                lastfspeed = player.Speed.Length();
                rate = player.Speed.Length() / speedconstant;
                if (neverSlowDown)
                {
                    rate = Math.Max(rate, 1);
                    lastfspeed = (float)Math.Max(speedconstant, lastfspeed);
                }
            }
            if (!conserveSpeedV && conserveSpeed)
            {
                if (dir.Y != 0 && dir.X != 0)
                {
                    const double vspeedSquared = speedconstant * speedconstant / 2;
                    double vspeed = speedconstant / Math.Sqrt(2);
                    var hspeed = Math.Sqrt(Math.Max(vspeedSquared, player.Speed.LengthSquared()) - vspeedSquared);
                    hspeed = Math.Max(hspeed, vspeed);
                    dir = new Vector2((float)hspeed * Math.Sign(dir.X), (float)vspeed * Math.Sign(dir.Y)).SafeNormalize();
                }
                else if (dir.Y != 0)
                {
                    rate = 1;
                    lastfspeed = (float)speedconstant;
                }
            }
            bool calcmethod()
            {
                if (lastfspeed > conserveCutOff)
                {
                    rate = player.Speed.Length() / lastfspeed * rate;
                }

                t = Calc.Approach(t, 1.0f, Engine.DeltaTime * 1.5f);

                float oldmov = mov;
                mov = (float)(60f * (float)Math.Sin(t * Math.PI));
                Vector2 delta = (mov - oldmov) * dir;

                playerData.Set(POSSIBLE_EARLY_DASHSPEED, earlyExitBoost = ((t > .6f) ? (t - .5f) * 200f * -dir : Vector2.Zero) * (float)rate);

                if (player.CollideCheck<Solid>(player.Position + delta))
                {
                    player.StateMachine.State = Player.StNormal;
                    return true;
                }
                //player.MoveH(delta.X);
                //player.MoveV(delta.Y);

                player.Speed = delta * 60f * (float)rate;
                lastfspeed = player.Speed.Length();
                return false;
            }
            if (!AnotherRedirect)
            {
                player.Speed = dir * (float)speedconstant * (float)rate;
                while (t < 0.5f)
                {
                    dir = player.Speed.SafeNormalize(Vector2.Zero);
                    if (calcmethod())
                    {
                        yield break;
                    }
                    yield return null;
                }
                mov = 60f;
                float oldmov_ = mov;
                Vector2 delta_ = (mov - oldmov_) * dir;
                //player.MoveH(delta_.X);
                //player.MoveV(delta_.Y);
                var backup = player.Speed;
                player.Speed = delta_ * 60f;
                yield return null; //what will happen if t == 0.5f ?
                player.Speed = -backup; //why?
                while (t < 1f)
                {
                    dir = (-player.Speed).SafeNormalize(Vector2.Zero);
                    if (calcmethod())
                    {
                        yield break;
                    }
                    yield return null;
                }
            }
            else
            {
                var dir_save = dir;
                player.Speed = dir_save * (float)speedconstant * (float)rate;
                while (t < 0.5f)
                {
                    dir = player.Speed.SafeNormalize(dir);
                    if (calcmethod())
                    {
                        yield break;
                    }
                    yield return null;
                }
                mov = 60f;
                float oldmov_ = mov;
                Vector2 delta_ = (mov - oldmov_) * dir;
                player.MoveH(delta_.X);
                player.MoveV(delta_.Y);
                //player.Speed = Vector2.Zero;
                //yield return null;
                player.Speed = -dir_save * (float)speedconstant * (float)rate;


                while (t < 1f)
                {
                    dir = (-player.Speed).SafeNormalize(dir);
                    if (calcmethod())
                    {
                        yield break;
                    }
                    yield return null;
                }
            }
            player.LiftSpeed += 120f * -dir * (float)rate;
            PurpleBoosterExplodeLaunch(player, playerData, player.Center - dir, origin, (float)rate);
        }

        private static void addDashAttack()
        {
            if (Util.TryGetPlayer(out Player player))
            {
                var data = player.GetData();
                if (/*DashAtt*/data.Get<bool>("USSRNAME_DashAtt"))
                {
                    data.Set("dashAttackTimer", usingDashAtt);
                }
            }
        }

        public static void PurpleBoosterExplodeLaunch(Player player, DynData<Player> playerData, Vector2 from, Vector2? origin, float factor = 1f)
        {
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Celeste.Freeze(0.1f);
            playerData.Set<float?>("launchApproachX", null);
            Level level = player.SceneAs<Level>();

            if (origin != null)
            {
                level.Displacement.AddBurst((Vector2)origin, 0.25f, 8f, 64f, 0.5f, Ease.QuadIn, Ease.QuadOut);
            }

            level.Shake(0.15f);

            Vector2 vector = (player.Center - from).SafeNormalize(-Vector2.UnitY);
            if (Math.Abs(vector.X) < 1f && Math.Abs(vector.Y) < 1f)
            {
                vector *= 1.1f;
            }

            player.Speed = 250f * -vector;

            Vector2 aim = Input.GetAimVector(player.Facing).EightWayNormal().Sign();
            if (aim.X == Math.Sign(player.Speed.X))
            {
                player.Speed.X *= 1.2f;
            }

            if (aim.Y == Math.Sign(player.Speed.Y))
            {
                player.Speed.Y *= 1.2f;
            }

            SlashFx.Burst(player.Center, player.Speed.Angle());
            if (!player.Inventory.NoRefills)
            {
                player.RefillDash();
            }
            player.RefillStamina();
            player.StateMachine.State = Player.StLaunch;
            player.Speed *= factor;
        }

        #endregion Custom Purple Booster Behavior

        internal static class Hooks
        {
            [SourceGen.Loader.Load]
            public static void Hook()
            {
                On.Celeste.Player.ctor += Player_ctor;

                //IL.Celeste.Player.Update += Player_Update;
            }

            //private static void Player_Update(MonoMod.Cil.ILContext il)
            //{

            //    if (Util.TryGetPlayer(out Player pl)
            //        && pl.StateMachine.State == ReverseHelperModule.AnotherPurpleBoosterDashState
            //        && /*FixSomething1*/pl.GetData().Get<bool>("USSRNAME_FixSomething1"))
            //    {
            //        var ilc = new ILCursor(il);
            //        while (ilc.TryGotoNext(x => x.MatchCall(typeof(Player), "DashCorrectCheck")))
            //        {
            //            //var ilc_last = ilc;
            //            ilc.Index -= 6;
            //            if (ilc.Instrs[ilc.Index].MatchCall(typeof(Actor), "MoveVExact"))
            //            {
            //                ilc.Instrs[ilc.Index].OpCode = Mono.Cecil.Cil.OpCodes.Pop;
            //                ilc.Instrs[ilc.Index].Operand = null;
            //                ilc.Index -= 1;
            //                ilc.Instrs[ilc.Index].OpCode = Mono.Cecil.Cil.OpCodes.Pop;
            //                ilc.Instrs[ilc.Index].Operand = null;
            //                //ilc.()
            //                return;
            //            }
            //        }
            //        Logger.Log(LogLevel.Error, "ReverseHelper/AnotherPurpleBooster", il.ToString());
            //        throw new RevverseHelperILHookException("IL Hooks failed! Method dump has been saved to log.");
            //        //ilc.nex
            //    }
            //}
            [SourceGen.Loader.Unload]
            public static void Unhook()
            {
                On.Celeste.Player.ctor -= Player_ctor;

                //IL.Celeste.Player.Update -= Player_Update;
            }

            private static void Player_ctor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
            {
                orig(self, position, spriteMode);
                // Custom Purple Booster State
                ReverseHelperModule.AnotherPurpleBoosterState = self.StateMachine.AddState(
                    new Func<int>(PurpleBoostUpdate),
                    PurpleBoostCoroutine,
                    PurpleBoostBegin,
                    PurpleBoostEnd);

                // Custom Purple Booster State (Arc Motion)
                ReverseHelperModule.AnotherPurpleBoosterDashState = self.StateMachine.AddState(
                    new Func<int>(PurpleDashingUpdate),
                    PurpleDashingCoroutine,
                    PurpleDashingBegin);
            }
        }
    }
}

namespace Celeste.Mod.ReverseHelper._From_Vortex_Helper
{
    public static class PlayerExt
    {
        private static DynData<Player> cachedPlayerData;

        public static DynData<Player> GetData(this Player player)
        {
            if (cachedPlayerData != null && cachedPlayerData.IsAlive && cachedPlayerData.Target == player)
            {
                return cachedPlayerData;
            }

            return cachedPlayerData = new DynData<Player>(player);
        }
    }

    // Thanks, Ja.
    // https://github.com/JaThePlayer/FrostHelper/blob/master/FrostTempleHelper/StateMachineExt.cs
    public static class StateMachineExt
    {
        /// <summary>
        /// Adds a state to a StateMachine
        /// </summary>
        /// <returns>The index of the new state</returns>
        public static int AddState(this StateMachine machine, Func<int> onUpdate, Func<IEnumerator> coroutine = null, Action begin = null, Action end = null)
        {
            Action[] begins = (Action[])StateMachine_begins.GetValue(machine);
            Func<int>[] updates = (Func<int>[])StateMachine_updates.GetValue(machine);
            Action[] ends = (Action[])StateMachine_ends.GetValue(machine);
            Func<IEnumerator>[] coroutines = (Func<IEnumerator>[])StateMachine_coroutines.GetValue(machine);
            int nextIndex = begins.Length;
            // Now let's expand the arrays
            Array.Resize(ref begins, begins.Length + 1);
            Array.Resize(ref updates, begins.Length + 1);
            Array.Resize(ref ends, begins.Length + 1);
            Array.Resize(ref coroutines, coroutines.Length + 1);
            // Store the resized arrays back into the machine
            StateMachine_begins.SetValue(machine, begins);
            StateMachine_updates.SetValue(machine, updates);
            StateMachine_ends.SetValue(machine, ends);
            StateMachine_coroutines.SetValue(machine, coroutines);
            // And now we add the new functions
            machine.SetCallbacks(nextIndex, onUpdate, coroutine, begin, end);
            return nextIndex;
        }

        private static FieldInfo StateMachine_begins = typeof(StateMachine).GetField("begins", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo StateMachine_updates = typeof(StateMachine).GetField("updates", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo StateMachine_ends = typeof(StateMachine).GetField("ends", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo StateMachine_coroutines = typeof(StateMachine).GetField("coroutines", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    public static class Util
    {
        // https://github.com/CommunalHelper/CommunalHelper/blob/dev/src/CommunalHelperModule.cs#L196
        public static bool TryGetPlayer(out Player? player)
        {
            player = Engine.Scene?.Tracker?.GetEntity<Player>();
            return player != null;
        }
    }
}