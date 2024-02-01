using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper._From_Vortex_Helper;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using YamlDotNet.Core.Tokens;

namespace Celeste.Mod.ReverseHelper
{
    [CustomEntity("ReverseHelper/ZiplineZipmover")]
    public class ZiplineZipmover : Entity
    {
        public const string NeverUsedZiplineFlag = "ReverseHelper_IsaGrabBag_NeverUsedZipline";

        private const float ZIP_SPEED = 120f;
        private const float ZIP_ACCEL = 190f;
        private const float ZIP_TURN = 250f;

        private static ZiplineZipmover? currentGrabbed, lastGrabbed;
        private static float ziplineBuffer;

        private readonly float height;
        private readonly Sprite? sprite;
        private readonly bool usesStamina;
        private Vector2 last_speed_mul_time_is_distance;
        private float last_speed_grace;
        private Vector2 speed_mul_time_is_distance;
        private Vector2 speed { get => speed_mul_time_is_distance / Engine.DeltaTime; }
        private Vector2 last_speed { get => last_speed_mul_time_is_distance / Engine.DeltaTime; }
        private bool grabbed;
        internal Vector2 target;
        internal Vector2 start;
        internal static Color ropeColor = Calc.HexToColor("663931");
        internal static Color ropeLightColor = Calc.HexToColor("9b6157");

        static Vector2 collide_offset;
        float speedRate = 2.0f;
        bool strict;
        public ZiplineZipmover(EntityData e, Vector2 offset)
            : base(e.Position + offset)
        {
            //LeftEdge = X;
            //RightEdge = X;
            //foreach (Vector2 node in _data.Nodes)
            //{
            //    LeftEdge = Math.Min(node.X + offset.X, LeftEdge);
            //    RightEdge = Math.Max(node.X + offset.X, RightEdge);
            //}
            //from = _data.Position + offset;
            target = e.Nodes[0] + offset;
            start = Position;
            strict = e.Bool("strict");
            float p = e.Float("time", -2);
            if (p <= 0)
            {
                p = e.Float("maxSpeed", -2);
                if (p > 0)
                {
                    //  distance / endspeed == 1 / (pi/2)
                    var distance = (target - start).Length();
                    var orig_endspeed = distance * (Math.PI / 2);
                    speedRate = (float)(p / orig_endspeed);
                }
            }
            else
            {
                speedRate = 1 / p;
            }
            usesStamina = e.Bool("usesStamina", true);
            //height = (_data.Position + offset).Y;
            Collider = new Hitbox(20, 16, -10, 1);
            currentGrabbed = null;
            Depth = -500;
            string s = e.Attr("sprite");
            if(s == "")
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
        }

        public static int ZiplineState { get; private set; }
        public static bool GrabbingCoroutine => currentGrabbed != null && !currentGrabbed.grabbed;
        public float LeftEdge { get; }
        public float RightEdge { get; }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(new ZiplineRender(this));
        }

        public override void Update()
        {
            base.Update();
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            Position += speed_mul_time_is_distance;

            if (player == null || player.Dead)
            {
                return;
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
                player.LiftSpeed = speed;
                player.MoveToX(Position.X - player.Collider.CenterX - collide_offset.X, onCollide);
                player.MoveToY(Position.Y + 22 - collide_offset.Y, onCollide);
            }
            else
            {
                if (currentGrabbed == null && player != null && !player.Dead && player.CanUnDuck && Input.GrabCheck && CanGrabZip(this) && (!strict || !player.CollideCheck<Solid>(new Vector2(Position.X - player.Collider.CenterX, Position.Y + 22))))
                {
                    bool isTired = (bool)Player_IsTired.GetValue(player);//DynamicData.For(player).Get<bool>("IsTired");
                    if (player.CollideCheck(this) && !isTired)
                    {
                        var pos = player.Position;
                        bool collide = false;
                        void onCollide(CollisionData data)
                        {
                            collide = true;
                        };

                        player.MoveToX(Position.X - player.Collider.CenterX,onCollide);
                        player.MoveToY(Position.Y + 22,onCollide);
                        if(strict&&collide)
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

            if (speed_mul_time_is_distance.LengthSquared() > last_speed_mul_time_is_distance.LengthSquared())
            {
                last_speed_mul_time_is_distance = speed_mul_time_is_distance;
                last_speed_grace = 0.15f;
            }
            speed_mul_time_is_distance = Vector2.Zero;
            last_speed_grace -= Engine.DeltaTime;
            if(last_speed_grace<0)
            {
                last_speed_mul_time_is_distance = Vector2.Zero;
            }
        }
        private IEnumerator Sequence()
        {
            Vector2 start = Position;
            while (true)
            {
                if (grabbed)
                {
                    sfx.Play("event:/game/01_forsaken_city/zip_mover", null, 0f);
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                    //this.StartShaking(0.1f);
                    yield return 0.1f;
                    //Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineIn, 0.5f, true);
                    float at = 0f;
                    while (at < 1f)
                    {
                        yield return null;
                        at = Calc.Approach(at, 1f, speedRate * Engine.DeltaTime);
                        percent = Ease.SineIn(at);
                        Vector2 vector = Vector2.Lerp(start, target, percent);
                        //this.ScrapeParticlesCheck(vector);
                        //if (this.Scene.OnInterval(0.1f))
                        //{
                        //    this.pathRenderer.CreateSparks();
                        //}
                        MoveSpeedTo(vector);

                    }

                    //this.StartShaking(0.2f);
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    SceneAs<Level>().Shake(0.3f);
                    //this.StopPlayerRunIntoAnimation = true;
                    yield return 0.5f;


                    //this.StopPlayerRunIntoAnimation = false;
                    //this.streetlight.SetAnimationFrame(2);
                    at = 0f;
                    while (at < 1f)
                    {
                        yield return null;
                        at = Calc.Approach(at, 1f, speedRate / 4 * Engine.DeltaTime);
                        percent = 1f - Ease.SineIn(at);
                        Vector2 position = Vector2.Lerp(target, start, Ease.SineIn(at));
                        MoveSpeedTo(position);
                    }
                    //this.StopPlayerRunIntoAnimation = true;
                    //this.StartShaking(0.2f);
                    //this.streetlight.SetAnimationFrame(1);

                    yield return 0.5f;
                }
                else
                {
                    yield return null;
                }
            }
        }

        private void MoveSpeedTo(Vector2 position)
        {
            var delta = position - Position;
            speed_mul_time_is_distance = delta;
        }

        public override void Render()
        {
            if (sprite is not null)
            {
                if (grabbed)
                {
                    //sprite.Visible = true;
                    sprite.Play(Engine.Scene.Tracker.GetEntity<Player>().Facing == Facings.Left ? "held_l" : "held_r");
                }
                else
                {
                    sprite.Play("idle");
                    //sprite.Visible = false;
                }
            }

            base.Render();
        }

        internal static void Load()
        {
            Everest.Events.Level.OnLoadLevel += Level_OnLoadLevel;
            //Everest.Events.Level.OnTransitionTo += Level_OnTransitionTo;
            On.Celeste.Player.ctor += PlayerInit;
            On.Celeste.Player.Update += OnPlayerUpdate;
            On.Celeste.Player.UpdateSprite += UpdatePlayerVisuals;
        }

        internal static void Unload()
        {
            Everest.Events.Level.OnLoadLevel -= Level_OnLoadLevel;
            //Everest.Events.Level.OnTransitionTo -= Level_OnTransitionTo;
            On.Celeste.Player.ctor -= PlayerInit;
            On.Celeste.Player.Update -= OnPlayerUpdate;
            On.Celeste.Player.UpdateSprite -= UpdatePlayerVisuals;
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

        private static void PlayerInit(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
        {
            orig(self, position, spriteMode);
            ZiplineState = self.StateMachine.AddState(ZiplineUpdate, ZiplineCoroutine, ZiplineBegin, ZiplineEnd);
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
            self.Speed = Vector2.Zero;
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

            //self.Speed = speed;

            //MoveEntityTo(self, playerLerp);
            //currentGrabbed.Position = zipLerp;


        }

        private static void ZiplineEnd()
        {
            Player self = Engine.Scene.Tracker.GetEntity<Player>();
            if(!s1mpleend)
            {
                self.Speed = currentGrabbed!.last_speed;
            }
            currentGrabbed!.grabbed = false;//anyway, it haven't crashed in grabbag.
            currentGrabbed = null;
            ziplineBuffer = 0.35f;
            s1mpleend = false;
        }
        static PropertyInfo Player_IsTired = typeof(Player).GetProperty("IsTired", BindingFlags.NonPublic | BindingFlags.Instance);
        //static FieldInfo Player_moveX = typeof(Player).GetField("moveX", BindingFlags.NonPublic | BindingFlags.Instance);
        private SoundSource sfx;
        internal float percent;
        static bool s1mpleend = false;
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
            from = ZipZip.start + new Vector2(ZipZip.Width / 2f, ZipZip.Height / 2f);
            to = ZipZip.target + new Vector2(ZipZip.Width / 2f, ZipZip.Height / 2f);
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
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void CreateSparks()
        {
            SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, from + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromA);
            SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, from - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromB);
            SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, to + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToA);
            SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, to - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToB);
        }

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
            Vector2 vector = (to - from).SafeNormalize();
            Vector2 value = vector.Perpendicular() * 3f;
            Vector2 value2 = -vector.Perpendicular() * 4f;
            float rotation = ZipZip.percent * 3.14159274f * 2f;
            Draw.Line(from + value + offset, to + value + offset, (colorOverride != null) ? colorOverride.Value : ZiplineZipmover.ropeColor);
            Draw.Line(from + value2 + offset, to + value2 + offset, (colorOverride != null) ? colorOverride.Value : ZiplineZipmover.ropeColor);
            for (float num = 4f - ZipZip.percent * 3.14159274f * 8f % 4f; num < (to - from).Length(); num += 4f)
            {
                Vector2 value3 = from + value + vector.Perpendicular() + vector * num;
                Vector2 value4 = to + value2 - vector * num;
                Draw.Line(value3 + offset, value3 + vector * 2f + offset, (colorOverride != null) ? colorOverride.Value : ZiplineZipmover.ropeLightColor);
                Draw.Line(value4 + offset, value4 - vector * 2f + offset, (colorOverride != null) ? colorOverride.Value : ZiplineZipmover.ropeLightColor);
            }
            cog.DrawCentered(from + offset, (colorOverride != null) ? colorOverride.Value : Color.White, 1f, rotation);
            cog.DrawCentered(to + offset, (colorOverride != null) ? colorOverride.Value : Color.White, 1f, rotation);
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
    //public class ZiplineRender : Entity
    //{
    //    private static readonly Color darkLine = Calc.HexToColor("9292a9");
    //    private static readonly Color lightLine = Calc.HexToColor("bbc0ce");

    //    private readonly List<RenderRectangle> renderList = new();
    //    private readonly ZiplineZipmover zipInst;
    //    private readonly Sprite? sprite;

    //    public ZiplineRender(ZiplineZipmover instance)
    //    {
    //        zipInst = instance;

    //        sprite = ReverseHelperExtern.IsaGrabBag.GrabBagModule.sprites?.Create("zipline");
    //        if(sprite is not null)
    //        {
    //            sprite.Play("idle");
    //            sprite.JustifyOrigin(new Vector2(0.5f, 0.25f));
    //            Add(sprite);
    //        }

    //        Depth = 500;
    //    }

    //    public override void Render()
    //    {
    //        renderList.Clear();

    //        Position = zipInst.Position;

    //        Rectangle tempRect = new((int)zipInst.LeftEdge, (int)zipInst.Y, (int)(zipInst.RightEdge - zipInst.LeftEdge), 1);
    //        tempRect.Inflate(8, 0);

    //        renderList.Add(new RenderRectangle(tempRect, darkLine));

    //        tempRect.Y -= 1;

    //        renderList.Add(new RenderRectangle(tempRect, lightLine));

    //        int left = tempRect.Left, right = tempRect.Right;

    //        renderList.Add(new RenderRectangle(new Rectangle(left - 2, (int)Y - 3, 2, 6), darkLine));
    //        renderList.Add(new RenderRectangle(new Rectangle(right, (int)Y - 3, 2, 6), darkLine));

    //        foreach (RenderRectangle rl in renderList)
    //        {
    //            Rectangle r = rl.rect;
    //            r.Inflate(1, 0);
    //            Draw.Rect(r, Color.Black);
    //            r.Inflate(-1, 1);
    //            Draw.Rect(r, Color.Black);
    //        }

    //        foreach (RenderRectangle rl in renderList)
    //        {
    //            Draw.Rect(rl.rect, rl.color);
    //        }

    //        base.Render();

    //    }
    //}

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