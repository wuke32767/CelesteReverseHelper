﻿using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper._From_Vortex_Helper;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;
using System.Collections;
using System.Runtime.CompilerServices;
using static Celeste.Mod.ReverseHelper.Libraries.EntityDataExt;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [CustomEntity("ReverseHelper/ZiplineZipmover")]
    public class ZiplineZipmover : Entity
    {
        private Shaker shaker;
        public EaseGroup CurrentEaserUnWrapped;
        public float CurrentEaser(float t) => t switch
        {
            < 1 => CurrentEaserUnWrapped.Easer(t),
            _ => (float)(1 + (t - 1) * CurrentEaserUnWrapped.EndSpeed)
        };

        public const string NeverUsedZiplineFlag = "ReverseHelper_IsaGrabBag_NeverUsedZipline";

        private const float ZIP_SPEED = 120f;
        private const float ZIP_ACCEL = 190f;
        private const float ZIP_TURN = 250f;
        private static ZiplineZipmover? currentGrabbed, lastGrabbed;
        private static float ziplineBuffer;

        //private readonly float height;
        private readonly Sprite? sprite;
        private readonly bool usesStamina;
        private Vector2 last_speed_mul_time_is_distance;
        private float last_speed_grace;
        private Vector2 speed_mul_time_is_distance;
        private Vector2 speed { get => speed_mul_time_is_distance / Engine.DeltaTime; }
        private Vector2 last_speed { get => last_speed_mul_time_is_distance / Engine.DeltaTime; }
        private bool grabbed;
        internal Vector2 currenttarget;
        internal Vector2 currentstart;
        internal Color ropeColor;
        internal Color ropeLightColor;


        float SinePosition = 0;
        float ConservedPosition = 0;
        enum Extension
        {
            None, SameDirection, All, AllWithDirection
        }
        bool IgnoreNegative;
        private readonly List<float>? Times;
        private readonly List<float>? Speeds;

        //public List<float> Times;

        float conservedSpeed = 0;
        Extension conserveSpeedMode;
        //bool anotherConserveWaiting;
        bool conserveMoving;
        bool conserveReturning;
        bool NoReturn;
        float extendRate;
        float extendLimit;

        bool fixEndSpeed;
        bool fixv3;

        bool acceptExtraSpeed = false;

        static Vector2 collide_offset;
        float speedRate = 2.0f;
        bool strict;
        static object? speedrun;
        Vector2 deltaNormalized;
        float deltaLength;
        internal Vector2[] NodeList;
        int pindex = 0;
        Vector2 MaxSpeed;//=>
                         //{
                         //    var distance = deltaLength;
                         //    var orig_endspeed = distance * (Math.PI / 2);
                         //    MaxSpeed = deltaNormalized* speedRate * (float) orig_endspeed;
                         //}//cached

        public ZiplineZipmover(EntityData e, Vector2 offset)
                : base(e.Position + offset)
        {
            shaker = new Shaker();
            CurrentEaserUnWrapped = e.EasePlus("easer");
            //LeftEdge = X;
            //RightEdge = X;
            //foreach (Vector2 node in _data.Nodes)
            //{
            //    LeftEdge = Math.Min(node.X + offset.X, LeftEdge);
            //    RightEdge = Math.Max(node.X + offset.X, RightEdge);
            //}
            //from = _data.Position + offset;
            currenttarget = e.Nodes[0] + offset;
            NodeList = e.Nodes.Prepend(e.Position).Select(x => x + offset).ToArray();
            currentstart = Position;
            deltaNormalized = currenttarget - currentstart;
            deltaLength = deltaNormalized.Length();
            deltaNormalized = deltaNormalized.SafeNormalize();
            strict = e.Bool("strict");
            conserveSpeedMode = e.Enum("conserveSpeedMode", Extension.None);
            extendRate = e.Float("conserveRate", 1);
            extendLimit = e.Float("conserveLimit", -1);
            fixEndSpeed = e.Bool("fixEndSpeed", false);
            conserveMoving = e.Bool("conserveMoving", false);
            conserveReturning = e.Bool("conserveReturning", false);
            IgnoreNegative = e.Bool("ignoreNegative", false);
            NoReturn = e.Bool("noReturn", false);
            var targetLen = NodeList.Length;
            if (NoReturn)
            {
                targetLen *= 2;
            }
            var Times = e.MergingList("time", floatParse, targetLen);
            var Speeds = e.MergingList("maxSpeed", floatParse, targetLen);

            var all = Times.Zip(Speeds);
            if (NoReturn)
            {
                var pre = all.Take(NodeList.Length);
                var post = all.Skip(NodeList.Length);
                all = pre.Concat(post.Zip(pre.Reverse()).Select(x =>
                {
                    if (x.Second.First is null && x.Second.Second is null)
                    {
                        return x.First;
                    }
                    else
                    {
                        return x.Second;
                    }
                }));
            }

            float last_time = 0.5f;
            float last_speed = -2;
            foreach (var ((time, speed), start, end) in all.Zip(NodeList, NodeList.Skip(1)))
            {
                var deltaNormalized = end - start;
                var deltaLength = deltaNormalized.Length();
                deltaNormalized = deltaNormalized.SafeNormalize();
                float t = time ?? -2;
                float ss = speed ?? -2;
            timer_:
                if (t > 0)
                {
                    speedRate = 1 / t;
                    var distance = deltaLength;
                    var orig_endspeed = distance * (Math.PI / 2);
                    MaxSpeed = deltaNormalized * speedRate * (float)orig_endspeed;
                }
                else
                {
                    if (ss > 0)
                    {
                        //  distance / endspeed == 1 / (pi/2)
                        var distance = deltaLength;
                        var orig_endspeed = distance * (Math.PI / 2);
                        speedRate = (float)(ss / orig_endspeed);
                        last_time = 1 / speedRate;
                        MaxSpeed = ss * deltaNormalized;
                    }
                    else
                    {
                        t = last_time;
                        ss = last_speed;
                        goto timer_;
                    }
                }

                speedRates.Add(speedRate);
                MaxSpeeds.Add(MaxSpeed);
                last_time = t;
                last_speed = ss;
            }
            UpdateSpeed(0);

            usesStamina = e.Bool("usesStamina", true);
            //height = (_data.Position + offset).Y;
            Collider = new Hitbox(20, 16, -10, 1);
            currentGrabbed = null;
            Depth = -500;
            string s = e.Attr("sprite");
            if (s == "")
            {
                sprite = ReverseHelperExtern.IsaGrabBag.GrabBagModule.sprites?.Create("zipline");
            }
            else
            {
                sprite = GFX.SpriteBank.Create(s);
            }
            if (sprite is not null)
            {
                sprite.Play("idle");
                sprite.JustifyOrigin(new Vector2(0.5f, 0.25f));
                Add(sprite);
            }
            Add(new Coroutine(Sequence()));
            sfx = new SoundSource();
            sfx.Position = new Vector2(Width, Height) / 2f;
            Add(sfx);

            fix2 = e.Bool("fixbugsv1", false);
            #region omni
            WaitingTimer = e.List("stoppings", EntityDataExt.floatParse, NodeList.Length, 0.4f).ToList();
            BootingTimer = e.List("startings", EntityDataExt.floatParse, NodeList.Length, 0.1f).ToList();
            WaitingTimerRet = e.List("returnStoppings", EntityDataExt.floatParse, NodeList.Length, 0.4f).ToList();
            BootingTimerRet = e.List("returnStartings", EntityDataExt.floatParse, NodeList.Length, 0.1f).ToList();

            WaitingTimerRet.Reverse();
            BootingTimerRet.Reverse();

            permanent = e.Bool("permanent");
            waiting = e.Bool("waiting");
            ticking = e.Bool("ticking");
            ticks = e.Int("ticks");
            tickDelay = e.Float("tickDelay");
            bool syncd = e.Bool("synced");
            var ropecol = e.Attr("ropeColor");
            if (string.IsNullOrWhiteSpace(ropecol))
            {
                ropecol = "663931";
            }

            ropeColor = Calc.HexToColor(ropecol);
            if (syncd)
            {
                var syncTag = e.Attr("syncTag");
                if (string.IsNullOrWhiteSpace(syncTag))
                {
                    sync = "ZipMoverSync:" + ropecol;
                }
                else
                {
                    sync = "ZipMoverSync:" + syncTag;
                }
            }
            fixv3 = e.Bool("fixbugsv2", false);
            ropeLightColor = e.HexColor("ropeLightColor", Calc.HexToColor("9b6157"));
            //this.nodeSpeeds=e.("nodeSpeeds");
            //this.startDelay=e.("startDelay");
            //this.customSound=e.("customSound");
            //this.startSound=e.("startSound");
            //this.impactSound=e.("impactSound");
            //this.tickSound=e.("tickSound");
            //this.returnSound=e.("returnSound");
            //this.finishSound=e.("finishSound");
            //this.nodeSound=e.("nodeSound");
            //this.startShaking=e.("startShaking");
            //this.nodeShaking=e.("nodeShaking");
            //this.returnShaking=e.("returnShaking");
            //this.tickingShaking=e.("tickingShaking");
            //this.permanentArrivalShaking=e.("permanentArrivalShaking");
            //this.tweakShakes=e.("tweakShakes");
            //this.hideRope=e.("hideRope");
            //this.hideCog=e.("hideCog");
            //this.dashable=e.("dashable");
            //this.onlyDashActivate=e.("onlyDashActivate");
            //this.dashableOnce=e.("dashableOnce");
            //this.dashableRefill=e.("dashableRefill");
            //this.tweakReturnParams=e.("tweakReturnParams");
            //this.returnSpeeds=e.("returnSpeeds");
            //this.returnedIrrespondingTime=e.("returnedIrrespondingTime");
            //this.returnEasing=e.("returnEasing");
            //this.timeUnits =e.("timeUnits ");
            void fill<Y>(List<Y> l)
            {
                var last = l[^1];
                l.AddRange(Enumerable.Repeat(last, NodeList.Length - l.Count));
            }
            #endregion
            SfxReset = e.Attr("resetSfx", "event:/ReverseHelper/VanillaTweaks/Zip_Unzipped/reset");
            SfxRet = e.Attr("returnSfx", "event:/ReverseHelper/VanillaTweaks/Zip_Unzipped/return");
            SfxImpact = e.Attr("impactSfx", "event:/ReverseHelper/VanillaTweaks/Zip_Unzipped/impact");
            SfxTouch = e.Attr("touchSfx", "event:/ReverseHelper/VanillaTweaks/Zip_Unzipped/touch");
            SfxTicking = e.Attr("tickingSfx", "event:/ReverseHelper/VanillaTweaks/Zip_Unzipped/touch");
            SfxDie = e.Attr("dieSfx", "event:/ReverseHelper/VanillaTweaks/Zip_Unzipped/touch");
        }
        private string SfxReset;
        private string SfxRet;
        private string SfxImpact;
        private string SfxTouch;
        private string SfxTicking;
        private string SfxDie;


        public static int ZiplineState { get; private set; }
        public static bool GrabbingCoroutine => currentGrabbed != null && !currentGrabbed.grabbed;
        public float LeftEdge { get; }
        public float RightEdge { get; }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (sprite is null)
            {
                RemoveSelf();
                return;
            }
            scene.Add(new ZiplineRender(this));
        }
        bool fixframe = false;
        public override void Update()
        {
            base.Update();
            Vector2 oPos = Position;
            var clamp = ConservedPosition + SinePosition;
            if (clamp < 0)
            {
                ConservedPosition = -SinePosition;
                clamp = 0;
                if (fix2 && fixframe)
                {
                }
                else
                {
                    conservedSpeed += (conservedSpeed + SineSpeed) / 2 * -3;
                }
            }
            else if (clamp > deltaLength)
            {
                ConservedPosition = deltaLength - SinePosition;
                clamp = deltaLength;
                if (fix2 && fixframe)
                {
                }
                else
                {
                    conservedSpeed += (conservedSpeed + SineSpeed) / 2 * -3;
                }
            }
            Position = Calc.Approach(currentstart, currenttarget, clamp);

            speed_mul_time_is_distance = Position - oPos;

            if (fixframe)
            {
                speed_mul_time_is_distance = (MaxSpeed * (returning ? (fix2 ? -0.25f : -1) : 1) + conservedSpeed * deltaNormalized) * Engine.DeltaTime;
            }
            fixframe = false;
            Player player = Engine.Scene.Tracker.GetEntity<Player>();

            if (player == null || player.Dead)
            {
                return;
            }
            last_speed_grace -= Engine.DeltaTime;
            if (last_speed_grace < 0)
            {
                last_speed_mul_time_is_distance = Vector2.Zero;
            }
            if (speed_mul_time_is_distance.LengthSquared() > last_speed_mul_time_is_distance.LengthSquared())
            {
                last_speed_mul_time_is_distance = speed_mul_time_is_distance;
                last_speed_grace = 0.15f - Engine.DeltaTime;
            }
            if (grabbed)
            {
                //if (Math.Abs(player.Speed.X) > 20)
                //{
                //    player.LiftSpeed = player.Speed;
                //    player.LiftSpeedGraceTime = 0.2f;
                //}
                //
                //if (player.CenterX > RightEdge || player.CenterX < LeftEdge)
                //{
                //    player.Speed.X = 0;
                //}
                //
                //player.CenterX = MathHelper.Clamp(player.CenterX, LeftEdge, RightEdge);
                //Position.X = player.CenterX;
                //Position.Y = height;

                void onCollide(CollisionData data)
                {
                    player.StateMachine.State = Player.StNormal;
                };
                player.LiftSpeed = /*speed*/fixEndSpeed switch
                {
                    true => last_speed,
                    false => speed,
                }; ;
                player.MoveToX(Position.X - player.Collider.CenterX - collide_offset.X, onCollide);
                player.MoveToY(Position.Y + 22 - collide_offset.Y, onCollide);
            }
            else
            {
                if (currentGrabbed == null && player != null && !player.Dead && player.CanUnDuck && Input.GrabCheck && CanGrabZip(this) && (!strict || !player.CollideCheck<Solid>(new Vector2(Position.X - player.Collider.CenterX, Position.Y + 22))))
                {
                    bool isTired = player.IsTired;//DynamicData.For(player).Get<bool>("IsTired");
                    if (player.CollideCheck(this) && !isTired)
                    {
                        var pos = player.Position;
                        bool collide = false;
                        void onCollide(CollisionData data)
                        {
                            collide = true;
                        };

                        player.MoveToX(Position.X - player.Collider.CenterX, onCollide);
                        player.MoveToY(Position.Y + 22, onCollide);
                        if (strict && collide)
                        {
                            player.Position = pos;
                        }
                        else
                        {
                            currentGrabbed = this;
                            lastGrabbed = currentGrabbed;
                            player.StateMachine.State = ZiplineState;
                            collide_offset = new Vector2(Position.X - player.Collider.CenterX, Position.Y + 22) - player.Position;
                        }
                    }
                }

                //Position += speed_mul_time;// * Engine.DeltaTime;
                //Position.X = MathHelper.Clamp(Position.X, LeftEdge, RightEdge);
                //Position.Y = height;
            }

            speed_mul_time_is_distance = Vector2.Zero;
        }
        bool returning = false;
        float SineSpeed;

        void CancelNextSeq()
        {
            currenttarget = NodeList[pindex];
            currentstart = NodeList[pindex - 1];
            deltaNormalized = currenttarget - currentstart;
            deltaLength = deltaNormalized.Length();
            deltaNormalized = deltaNormalized.SafeNormalize();

            UpdateSpeed(pindex - 1);
        }
        void ResetSeq()
        {
            pindex = 0;
            currenttarget = NodeList[1];
            currentstart = NodeList[0];
            deltaNormalized = currenttarget - currentstart;
            deltaLength = deltaNormalized.Length();
            deltaNormalized = deltaNormalized.SafeNormalize();

            UpdateSpeed(pindex);
        }

        bool NextSeq()
        {
            pindex++;
            if (pindex + 1 >= NodeList.Length)
            {
                return false;
            }
            currenttarget = NodeList[pindex + 1];
            currentstart = NodeList[pindex];
            deltaNormalized = currenttarget - currentstart;
            deltaLength = deltaNormalized.Length();
            deltaNormalized = deltaNormalized.SafeNormalize();

            UpdateSpeed(pindex);
            return true;
        }

        bool PrevSeq()
        {
            void PrevSeqMethod() => CancelNextSeq();
            pindex--;
            if (pindex <= 0)
            {
                return false;
            }
            PrevSeqMethod();

            return true;
        }
        void UpdateSpeed(int i)
        {
            //if (speedRates.Count > i)
            //{
            //    static float reader(List<float>? from, int index)
            //    {
            //        if (from?.Count > index)
            //        {
            //            return from[index];
            //        }
            //        return -2;
            //    }
            //    float p = reader(Times, i);
            //timer_:
            //    if (p > 0)
            //    {
            //        speedRate = 1 / p;
            //        var distance = deltaLength;
            //        var orig_endspeed = distance * (Math.PI / 2);
            //        MaxSpeed = deltaNormalized * speedRate * (float)orig_endspeed;
            //    }
            //    else
            //    {
            //        p = reader(Speeds, i);
            //        if (p > 0)
            //        {
            //            //  distance / endspeed == 1 / (pi/2)

            //            var distance = deltaLength;
            //            var orig_endspeed = distance * (Math.PI / 2);
            //            speedRate = (float)(p / orig_endspeed);
            //            MaxSpeed = p * deltaNormalized;
            //        }
            //        else
            //        {
            //            p = 0.5f;
            //            goto timer_;
            //        }
            //    }

            //    speedRates.Add(speedRate);
            //    MaxSpeeds.Add(MaxSpeed);
            //}
            //else
            {
                speedRate = speedRates[i];
                MaxSpeed = MaxSpeeds[i];
            }
        }
        string? sync = null;
        bool shouldTrigger => grabbed || (sync is not null && SceneAs<Level>().Session.GetFlag(sync));
        void MakeSync(bool flag)
        {
            if (sync is not null)
            {
                SceneAs<Level>()?.Session.SetFlag(sync, flag);
            }
        }
        private IEnumerator Sequence()
        {

            while (true)
            {
                SineSpeed = 0;
                returning = false;
                acceptExtraSpeed = true;
                if (!shouldTrigger)
                {
                    conservedSpeed = 0;
                    ConservedPosition = 0;
                    yield return null;
                    continue;
                }

                float at = 0f;
                do
                {
                    MakeSync(true);
                    at = 0;
                    sfx.Play(SfxTouch, null, 0f);

                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                    SinePosition = ConservedPosition = 0;
                    //this.StartShaking(0.1f);
                    yield return BootingTimer[pindex];
                    MakeSync(false);
                    acceptExtraSpeed = conserveMoving;
                    if (conservedSpeed < 0)
                    {
                        conservedSpeed = 0;
                    }

                    //Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineIn, 0.5f, true);
                    while (ConservedPosition + SinePosition < deltaLength)
                    {
                        yield return null;
                        ConservedPosition += conservedSpeed * Engine.DeltaTime;
                        if (conservedSpeed < 0)
                        {
                            conservedSpeed += MaxSpeed.Length() * Engine.DeltaTime;
                        }
                        at += speedRate * Engine.DeltaTime;
                        percent = CurrentEaser(at);

                        float position = deltaLength * percent;
                        //this.ScrapeParticlesCheck(vector);
                        //if (this.Scene.OnInterval(0.1f))
                        //{
                        //    this.pathRenderer.CreateSparks();
                        //}
                        SineSpeed = (position - SinePosition) / Engine.DeltaTime;
                        SinePosition = position;
                    }
                    sfx.Play(SfxImpact, null, 0f);
                    //this.StartShaking(0.2f);
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    SceneAs<Level>().Shake(0.3f);
                    fixframe = fixEndSpeed;
                    //this.StopPlayerRunIntoAnimation = true;
                    yield return WaitingTimer[pindex];
                    //SinePosition = deltaLength;
                    //ConservedPosition = 0;
                    conservedSpeed = 0;
                    acceptExtraSpeed = true;
                    if (!NextSeq())
                    {
                        if (NoReturn)
                        {
                            ResetSeq();
                        }
                        else
                        {
                            break;
                        }
                    }
                    SinePosition = ConservedPosition = 0;
                    if (waiting)
                    {
                        while (!shouldTrigger)
                        {
                            conservedSpeed = 0;
                            yield return null;
                        }
                    }
                    else if (ticking)
                    {
                        int tick = 0;
                        float waitings = 0;
                        while (!shouldTrigger && ticks > tick)
                        {
                            waitings += Engine.DeltaTime;
                            if (waitings >= tickDelay)
                            {
                                waitings = 0;
                                tick++;
                                Audio.Play(SfxTicking);
                            }
                            conservedSpeed = 0;
                            yield return null;
                        }
                        if (!shouldTrigger)
                        {
                            if (permanent)
                            {
                                Audio.Play(SfxDie);
                                yield break;
                            }
                            CancelNextSeq();
                            break;
                        }
                    }
                } while (true);
                if (permanent)
                {
                    yield break;
                }
                returning = true;
                do
                {
                    acceptExtraSpeed = conserveReturning;
                    sfx.Play(SfxRet, null, 0f);
                    ConservedPosition = 0;
                    SinePosition = deltaLength;
                    yield return BootingTimerRet[pindex];
                    if (conservedSpeed > 0)
                    {
                        conservedSpeed = 0;
                    }
                    //this.StopPlayerRunIntoAnimation = false;
                    //this.streetlight.SetAnimationFrame(2);
                    at = 0f;
                    while (ConservedPosition + SinePosition > 0)
                    {
                        yield return null;
                        ConservedPosition += conservedSpeed * Engine.DeltaTime;
                        if (conservedSpeed > 0)
                        {
                            conservedSpeed -= MaxSpeed.Length() * Engine.DeltaTime / 4;
                        }

                        at += speedRate * Engine.DeltaTime / 4;
                        percent = 1f - CurrentEaser(at);
                        float position = MathHelper.Lerp(deltaLength, 0, CurrentEaser(at));
                        SineSpeed = (position - SinePosition) / Engine.DeltaTime;
                        SinePosition = position;

                    }
                    sfx.Play(SfxReset, null, 0f);
                    //this.StopPlayerRunIntoAnimation = true;
                    //this.StartShaking(0.2f);
                    //this.streetlight.SetAnimationFrame(1);
                    conservedSpeed = 0;
                    acceptExtraSpeed = false;
                    fixframe = fixEndSpeed;
                    yield return WaitingTimerRet[pindex];
                } while (PrevSeq());
                returning = false;
                yield return 0.1f;
                SinePosition = ConservedPosition = 0;
                conservedSpeed = 0;

            }
        }


        public override void Render()
        {
            if (sprite is not null)
            {
                if (grabbed)
                {
                    //sprite.Visible = true;
                    var p = Engine.Scene.Tracker.GetEntity<Player>();
                    if (p is not null)
                    {
                        sprite.Play(p.Facing == Facings.Left ? "held_l" : "held_r");
                    }
                }
                else
                {
                    sprite.Play("idle");
                    //sprite.Visible = false;
                }
            }

            base.Render();
        }
        [SourceGen.Loader.Load]

        internal static void Load()
        {
            speedrun = ReverseHelperExtern.SpeedRunTool_Interop.RegisterStaticTypes?.Invoke(typeof(ZiplineZipmover), [nameof(currentGrabbed), nameof(ZiplineState)]);

            Everest.Events.Level.OnLoadLevel += Level_OnLoadLevel;
            //Everest.Events.Level.OnTransitionTo += Level_OnTransitionTo;
            Everest.Events.Player.OnRegisterStates += PlayerInit2;
            //On.Celeste.Player.ctor += PlayerInit;
            On.Celeste.Player.Update += OnPlayerUpdate;
            On.Celeste.Player.UpdateSprite += UpdatePlayerVisuals;
        }

        [SourceGen.Loader.Unload]
        internal static void Unload()
        {
            Everest.Events.Level.OnLoadLevel -= Level_OnLoadLevel;
            //Everest.Events.Level.OnTransitionTo -= Level_OnTransitionTo;
            Everest.Events.Player.OnRegisterStates -= PlayerInit2;
            //On.Celeste.Player.ctor -= PlayerInit;
            On.Celeste.Player.Update -= OnPlayerUpdate;
            On.Celeste.Player.UpdateSprite -= UpdatePlayerVisuals;
            ReverseHelperExtern.SpeedRunTool_Interop.Unregister?.Invoke(speedrun!);
        }

        private static void Level_OnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            currentGrabbed = lastGrabbed = null;
            //if (isFromLoader && (level.Session.StartedFromBeginning || level.Session.RestartedFromGolden) && level.Session.MapData.HasEntity("ReverseHelper/ZiplineZipmover"))
            //{
            //    level.Session.SetFlag(NeverUsedZiplineFlag, true);
            //}
        }

        private static void Level_OnTransitionTo(Level level, LevelData next, Vector2 direction)
        {
            //if (lastGrabbed != null)
            //{
            //    level.Session.SetFlag(NeverUsedZiplineFlag, false);
            //}
        }

        //private static void PlayerInit(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
        //{
        //    orig(self, position, spriteMode);
        //    ZiplineState = self.StateMachine.AddState(ZiplineUpdate, ZiplineCoroutine, ZiplineBegin, ZiplineEnd);
        //}
        private static void PlayerInit2(Player self)
        {
            ZiplineState = self.StateMachine.AddState("ZiplineZipMover", ZiplineUpdate, ZiplineCoroutine, ZiplineBegin, ZiplineEnd);
        }

        private static void OnPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);

            ziplineBuffer = Calc.Approach(ziplineBuffer, 0, Engine.DeltaTime);
            if (!Input.GrabCheck)
            {
                ziplineBuffer = 0;
            }
        }

        private static void UpdatePlayerVisuals(On.Celeste.Player.orig_UpdateSprite orig, Player self)
        {
            if (ZiplineState > Player.StIntroThinkForABit && self.StateMachine == ZiplineState)
            {
                self.Sprite.Scale.X = Calc.Approach(self.Sprite.Scale.X, 1f, 1.75f * Engine.DeltaTime);
                self.Sprite.Scale.Y = Calc.Approach(self.Sprite.Scale.Y, 1f, 1.75f * Engine.DeltaTime);

                if (GrabbingCoroutine)
                {
                    return;
                }

                self.Sprite.PlayOffset("fallSlow_carry", .5f, false);
                self.Sprite.Rate = 0.0f;
            }
            else
            {
                orig(self);
            }
        }

        private static void ZiplineBegin()
        {
            Player self = Engine.Scene.Tracker.GetEntity<Player>();
            self.Ducking = false;
            //self.Speed.Y = 0;



            //Player self = Engine.Scene.Tracker.GetEntity<Player>();
            //Vector2 speed = self.Speed;
            //currentGrabbed.speed_mul_time = 0;

            self.Sprite.Play("pickup");

            self.Play("event:/char/madeline/crystaltheo_lift", null, 0f);

            //Vector2 playerLerp = new((self.X + currentGrabbed.X) / 2f, currentGrabbed.Y + 22);

            //playerLerp.X = currentGrabbed.X;//MathHelper.Clamp(playerLerp.X, currentGrabbed.LeftEdge, currentGrabbed.RightEdge);
            //Vector2 zipLerp = new(playerLerp.X, currentGrabbed.Y);

            //Vector2 playerInit = self.Position,
            //    zipInit = currentGrabbed.Position;

            ////Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, 0.07f, true);

            ////while (tween.Active)
            //{
            //    //tween.Update();

            //    MoveEntityTo(self, Vector2.Lerp(playerInit, playerLerp, 1));
            //    //currentGrabbed.Position = Vector2.Lerp(zipInit, zipLerp, 1);

            //    //yield return null;
            //}

            currentGrabbed!.grabbed = true;
            if (currentGrabbed.acceptExtraSpeed)
            {
                var del = currentGrabbed.deltaNormalized;
                var dirx = self.Speed.X * del.X + self.Speed.Y * del.Y;
                if (dirx == 0)
                {
                    dirx = (currentGrabbed.returning ? -1 : 1);
                }
                else
                {
                    dirx = Math.Sign(dirx);
                }
                var res = currentGrabbed.conserveSpeedMode switch
                {
                    Extension.SameDirection => self.Speed.X * del.X + self.Speed.Y * del.Y,
                    Extension.All => (currentGrabbed.returning ? -1 : 1) * self.Speed.Length(),
                    Extension.AllWithDirection => dirx * self.Speed.Length(),
                    _ => 0,
                };
                if (Math.Abs(res) > currentGrabbed.extendLimit && currentGrabbed.extendLimit >= 0)
                {
                    res = Math.Sign(res) * currentGrabbed.extendLimit;
                }
                //if(currentGrabbed.returning)
                //{
                //    res = -res;
                //}
                if (currentGrabbed.IgnoreNegative && (res > 0 == currentGrabbed.returning/*(res>0&&currentGrabbed.returning)||(res<0&&!currentGrabbed.returning)*/))
                {

                }
                else
                {
                    currentGrabbed.conservedSpeed += res * currentGrabbed.extendRate;
                }
            }
            self.Speed = Vector2.Zero;
            //self.Speed = speed;

            //MoveEntityTo(self, playerLerp);
            //currentGrabbed.Position = zipLerp;


        }

        private static void ZiplineEnd()
        {
            Player self = Engine.Scene.Tracker.GetEntity<Player>();
            if (!s1mpleend)
            {
                self.Speed = currentGrabbed!.last_speed;
            }
            currentGrabbed!.grabbed = false;//anyway, it haven't crashed in grabbag.
            currentGrabbed = null;
            ziplineBuffer = 0.35f;
            s1mpleend = false;
        }
        //static FieldInfo Player_moveX = typeof(Player).GetField("moveX", BindingFlags.NonPublic | BindingFlags.Instance);
        private SoundSource sfx;
        internal float percent;
        static bool s1mpleend = false;
        private bool fix2;
        private List<float> WaitingTimer;
        private List<float> BootingTimer;
        private List<float> WaitingTimerRet;
        private List<float> BootingTimerRet;
        private bool permanent;
        private bool waiting;
        private bool ticking;
        private int ticks;
        private float tickDelay;
        private List<float> speedRates = [];
        private List<Vector2> MaxSpeeds = [];

        private static int ZiplineUpdate()
        {
            Player self = Engine.Scene.Tracker.GetEntity<Player>();

            if (currentGrabbed == null)
            {
                return Player.StNormal;
            }

            if (!currentGrabbed.grabbed)
            {
                return ZiplineState;
            }

            //currentGrabbed.speed_mul_time = self.Speed.X;

            //if (Math.Sign(self.LiftSpeed.X) * Math.Sign(self.Speed.X) == -1 || Math.Abs(self.LiftSpeed.X) <= Math.Abs(self.Speed.X))
            //{
            //    self.LiftSpeed = self.Speed;
            //    self.LiftSpeedGraceTime = 0.15f;
            //}

            //int moveX = (int)Player_moveX.GetValue(self);//DynamicData.For(self).Get<int>("moveX");
            //if (Math.Sign(moveX) == -Math.Sign(self.Speed.X))
            //{
            //    self.Speed.X = Calc.Approach(self.Speed.X, moveX * ZIP_SPEED, ZIP_TURN * Engine.DeltaTime);
            //}
            //else if (Math.Abs(self.Speed.X) <= ZIP_SPEED || Math.Sign(moveX) != Math.Sign(self.Speed.X))
            //{
            //    self.Speed.X = Calc.Approach(self.Speed.X, moveX * ZIP_SPEED, ZIP_ACCEL * Engine.DeltaTime);
            //}

            if (!Input.GrabCheck || self.Stamina <= 0)
            {
                //self.Speed = currentGrabbed.last_speed;
                return Player.StNormal;
            }

            if (Input.Jump.Pressed)
            {
                Input.Jump.ConsumePress();

                if (currentGrabbed.usesStamina)
                {
                    self.Stamina -= 110f / 8f;
                }

                self.Speed = currentGrabbed.last_speed;
                self.Speed.X *= 0.1f;
                self.Jump(false, true);
                self.LiftSpeed *= 0.4f;
                s1mpleend = true;
                //currentGrabbed.speed_mul_time = Calc.Approach(currentGrabbed.speed_mul_time, 0, 20);

                return Player.StNormal;
            }

            if (self.CanDash)
            {
                self.Speed = currentGrabbed.last_speed;
                s1mpleend = true;
                return self.StartDash();
            }

            if (currentGrabbed.usesStamina)
            {
                self.Stamina -= 5 * Engine.DeltaTime;
            }

            return ZiplineState;
        }

        private static IEnumerator ZiplineCoroutine()
        {
            yield break;
        }

        private static void MoveEntityTo(Actor ent, Vector2 position)
        {
            ent.MoveToX(position.X);
            ent.MoveToY(position.Y);
        }

        private static bool CanGrabZip(ZiplineZipmover line)
        {
            return lastGrabbed != line || ziplineBuffer <= 0;
        }

    }
    class ZiplineRender : Entity
    {
        // Token: 0x06002FCA RID: 12234 RVA: 0x0011B4E4 File Offset: 0x001196E4
        [MethodImpl(MethodImplOptions.NoInlining)]
        public ZiplineRender(ZiplineZipmover zipMover)
        {
            Depth = 5000;
            ZipZip = zipMover;
            from = ZipZip.currentstart + new Vector2(ZipZip.Width / 2f, ZipZip.Height / 2f);
            to = ZipZip.currenttarget + new Vector2(ZipZip.Width / 2f, ZipZip.Height / 2f);
            sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
            float num = (from - to).Angle();
            sparkDirFromA = num + 0.3926991f;
            sparkDirFromB = num - 0.3926991f;
            sparkDirToA = num + 3.14159274f - 0.3926991f;
            sparkDirToB = num + 3.14159274f + 0.3926991f;
            //if (zipMover.theme == ZipMover.Themes.Moon)
            //{
            //    this.cog = GFX.Game["objects/zipmover/moon/cog"];
            //    return;
            //}
            cog = GFX.Game["objects/zipmover/cog"];
        }

        // Token: 0x06002FCB RID: 12235 RVA: 0x0011B634 File Offset: 0x00119834
        //[MethodImpl(MethodImplOptions.NoInlining)]
        //public void CreateSparks()
        //{
        //    SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, from + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromA);
        //    SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, from - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromB);
        //    SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, to + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToA);
        //    SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, to - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToB);
        //}

        // Token: 0x06002FCC RID: 12236 RVA: 0x0011B76C File Offset: 0x0011996C
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Render()
        {
            DrawCogs(new Vector2(0 - 10, 1 - 8), new Color?(Color.Black));
            DrawCogs(new Vector2(0 - 10, 0 - 8), null);
            //if (this.ZipMover.drawBlackBorder)
            //{
            //    Draw.Rect(new Rectangle((int)(this.ZipMover.X + this.ZipMover.Shake.X - 1f), (int)(this.ZipMover.Y + this.ZipMover.Shake.Y - 1f), (int)this.ZipMover.Width + 2, (int)this.ZipMover.Height + 2), Color.Black);
            //}
        }

        // Token: 0x06002FCD RID: 12237 RVA: 0x0011B820 File Offset: 0x00119A20
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void DrawCogs(Vector2 offset, Color? colorOverride = null)
        {
            offset += new Vector2(ZipZip.Width / 2f, ZipZip.Height / 2f);
            float rotation = ZipZip.percent * 3.14159274f * 2f;
            foreach (var (from, to) in ZipZip.NodeList.Zip(ZipZip.NodeList.Skip(1)))
            {
                Vector2 vector = (to - from).SafeNormalize();
                Vector2 value = vector.Perpendicular() * 3f;
                Vector2 value2 = -vector.Perpendicular() * 4f;
                Draw.Line(from + value + offset, to + value + offset, (colorOverride != null) ? colorOverride.Value : ZipZip.ropeColor);
                Draw.Line(from + value2 + offset, to + value2 + offset, (colorOverride != null) ? colorOverride.Value : ZipZip.ropeColor);
                for (float num = 4f - ZipZip.percent * 3.14159274f * 8f % 4f; num < (to - from).Length(); num += 4f)
                {
                    Vector2 value3 = from + value + vector.Perpendicular() + vector * num;
                    Vector2 value4 = to + value2 - vector * num;
                    Draw.Line(value3 + offset, value3 + vector * 2f + offset, (colorOverride != null) ? colorOverride.Value : ZipZip.ropeLightColor);
                    Draw.Line(value4 + offset, value4 - vector * 2f + offset, (colorOverride != null) ? colorOverride.Value : ZipZip.ropeLightColor);
                }
            }
            foreach (var from in ZipZip.NodeList)
            {
                cog.DrawCentered(from + offset, (colorOverride != null) ? colorOverride.Value : Color.White, 1f, rotation);
                //cog.DrawCentered(to + offset, (colorOverride != null) ? colorOverride.Value : Color.White, 1f, rotation);
            }
        }

        // Token: 0x04002923 RID: 10531
        public ZiplineZipmover ZipZip;

        // Token: 0x04002924 RID: 10532
        private MTexture cog;

        // Token: 0x04002925 RID: 10533
        private Vector2 from;

        // Token: 0x04002926 RID: 10534
        private Vector2 to;

        // Token: 0x04002927 RID: 10535
        private Vector2 sparkAdd;

        // Token: 0x04002928 RID: 10536
        private float sparkDirFromA;

        // Token: 0x04002929 RID: 10537
        private float sparkDirFromB;

        // Token: 0x0400292A RID: 10538
        private float sparkDirToA;

        // Token: 0x0400292B RID: 10539
        private float sparkDirToB;
    }

    public struct RenderRectangle
    {
        public Rectangle rect;
        public Color color;

        public RenderRectangle(Rectangle r, Color c)
        {
            rect = r;
            color = c;
        }
    }
    public static class Utils
    {
        public static float Mod(float x, float m)
        {
            return ((x % m) + m) % m;
        }

        public static EntityData? GetEntityData(this MapData mapData, string entityName)
        {
            foreach (LevelData levelData in mapData.Levels)
            {
                if (levelData.GetEntityData(entityName) is EntityData entityData)
                {
                    return entityData;
                }
            }

            return null;
        }

        public static bool HasEntity(this MapData mapData, string entityName)
        {
            return mapData.GetEntityData(entityName) != null;
        }

        public static EntityData? GetEntityData(this LevelData levelData, string entityName)
        {
            foreach (EntityData entity in levelData.Entities)
            {
                if (entity.Name == entityName)
                {
                    return entity;
                }
            }

            return null;
        }
    }

}