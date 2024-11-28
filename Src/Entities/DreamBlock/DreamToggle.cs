using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [CustomEntity("ReverseHelper/DreamToggle")]
    public class DreamToggle : Entity
    {

        public static Color activeBackColor = Color.Black;

        public static Color disabledBackColor = Calc.HexToColor("1f2e2d");

        public static Color activeLineColor = Color.White;

        public static Color disabledLineColor = Calc.HexToColor("6a8480");
        bool v_on;
        public static float EstimateTime(Sprite.Animation animation)
        {
            return animation.Delay * animation.Frames.Length;
        }
        bool v2;
        public DreamToggle(Vector2 position, bool onlyFire, bool onlyIce, bool persistent, int depth, bool v2) : base(position)
        {
            onlyDisable = onlyFire;
            onlyEnable = onlyIce;
            this.persistent = persistent = true;
            Collider = new Hitbox(16f, 24f, -8f, -12f);
            Add(new DreamToggleListener(OnChangeMode));
            Add(new PlayerCollider((OnPlayer), null, null));
            this.v2 = v2;
            if (v2)
            {
                Add(sprite = GFX.SpriteBank.Create("ReverseHelper_DreamToggleV2"));

                sprite.OnChange = (last, cur) =>
                {
                    switch (cur)
                    {
                        case "enable":
                            Add(new Coroutine(ActivateRoutine(EstimateTime(sprite.Animations["enable"]))));
                            break;
                        case "enableLoop":
                            break;
                        case "enableOff":
                            break;
                        case "enableOffLoop":
                            break;
                        case "disable":
                            Add(new Coroutine(DeactivateRoutine(EstimateTime(sprite.Animations["enable"]))));
                            break;
                        case "disableLoop":

                            break;
                        case "disableOff":
                            break;
                        case "disableOffLoop":
                            break;
                    }
                };
            }
            else
            {
                Add(sprite = GFX.SpriteBank.Create("ReverseHelper_DreamToggle"));
            }
            Depth = depth;
        }

        private IEnumerator DeactivateRoutine(float v)
        {
            throw new NotImplementedException();
        }

        private IEnumerator ActivateRoutine(float v)
        {
            throw new NotImplementedException();
        }


        // Token: 0x0600164F RID: 5711 RVA: 0x0005C3BA File Offset: 0x0005A5BA
        public DreamToggle(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("onlyEnable", false), data.Bool("onlyDisable", false), data.Bool("persistent_always_true", true),
                  data.Int("depth", 2000), data.Bool("neo", false))
        {
        }

        // Token: 0x06001650 RID: 5712 RVA: 0x0005C3F2 File Offset: 0x0005A5F2

        public override void Added(Scene scene)
        {
            base.Added(scene);
            enableMode = ReverseHelperModule.playerHasDreamDashBetter(this);
            SetSprite(false);
        }

        // Token: 0x06001651 RID: 5713 RVA: 0x0005C416 File Offset: 0x0005A616
        private void OnChangeMode(bool enabled)
        {
            bool changed = enableMode != ReverseHelperModule.playerHasDreamDashBetter(this);
            enableMode = ReverseHelperModule.playerHasDreamDashBetter(this);
            SetSprite(changed);
        }
        public new TilesetDreamBlock.DreamParticle[] particles = null!;
        public void Setup()
        {
            var X = sprite.Center.X - 6;
            var Y = sprite.Center.Y - 6;
            var Width = 12;
            var Height = 12;

            particles = new TilesetDreamBlock.DreamParticle[(int)(Width / 8f * (Height / 8f) * 0.7f)];
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Position = new Vector2(Calc.Random.NextFloat(Width), Calc.Random.NextFloat(Height));
                particles[i].Layer = Calc.Random.Choose(0, 1, 1, 2, 2, 2);
                particles[i].TimeOffset = Calc.Random.NextFloat();
                particles[i].Colord = Color.LightGray * (0.5f + particles[i].Layer / 2f * 0.5f);
                //if (playerHasDreamDash)
                {
                    switch (particles[i].Layer)
                    {
                        case 0:
                            particles[i].Colora = Calc.Random.Choose(Calc.HexToColor("FFEF11"), Calc.HexToColor("FF00D0"), Calc.HexToColor("08a310"));
                            break;
                        case 1:
                            particles[i].Colora = Calc.Random.Choose(Calc.HexToColor("5fcde4"), Calc.HexToColor("7fb25e"), Calc.HexToColor("E0564C"));
                            break;
                        case 2:
                            particles[i].Colora = Calc.Random.Choose(Calc.HexToColor("5b6ee1"), Calc.HexToColor("CC3B3B"), Calc.HexToColor("7daa64"));
                            break;
                    }
                }
            }
        }
        MTexture[] particleTextures =
           [
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(14, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(0, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7)
           ];

        public override void Render()
        {
            if (v2)
            {
                Vector2 PutInside(Vector2 pos)
                {
                    var X = sprite.Center.X - 6;
                    var Y = sprite.Center.Y - 6;
                    var Width = 12;
                    var Height = 12;
                    var Right = X + Width;
                    var Left = X;
                    var Top = Y;
                    var Bottom = Y + Height;
                    if (pos.X > Right)
                    {
                        pos.X -= (float)Math.Ceiling((pos.X - Right) / Width) * Width;
                    }
                    else if (pos.X < Left)
                    {
                        pos.X += (float)Math.Ceiling((Left - pos.X) / Width) * Width;
                    }

                    if (pos.Y > Bottom)
                    {
                        pos.Y -= (float)Math.Ceiling((pos.Y - Bottom) / Height) * Height;
                    }
                    else if (pos.Y < Top)
                    {
                        pos.Y += (float)Math.Ceiling((Top - pos.Y) / Height) * Height;
                    }

                    return pos;
                }

                Camera camera = SceneAs<Level>().Camera;
                var X = sprite.Center.X - 6;
                var Y = sprite.Center.Y - 6;
                var Width = 12;
                var Height = 12;
                var Right = X + Width;
                var Left = X;
                var Top = Y;
                var Bottom = Y + Height;
                if (Right < camera.Left || Left > camera.Right || Bottom < camera.Top || Top > camera.Bottom)
                {
                    return;
                }

                Draw.Rect(X, Y, Width, Height, playerHasDreamDash ? DreamBlock.activeBackColor : DreamBlock.disabledBackColor);
                Vector2 position = SceneAs<Level>().Camera.Position;
                for (int i = 0; i < particles.Length; i++)
                {
                    int layer = particles[i].Layer;
                    Vector2 vector = particles[i].Position;
                    vector += position * (0.3f + 0.25f * (float)layer);
                    vector = PutInside(vector);
                    Color color = particles[i].Color(true);
                    MTexture mtexture;
                    if (layer == 0)
                    {
                        int num = (int)((particles[i].TimeOffset * 4f + animTimer) % 4f);
                        mtexture = particleTextures[3 - num];
                    }
                    else if (layer == 1)
                    {
                        int num2 = (int)((particles[i].TimeOffset * 2f + animTimer) % 2f);
                        mtexture = particleTextures[1 + num2];
                    }
                    else
                    {
                        mtexture = particleTextures[2];
                    }
                    if (vector.X >= X + 2f && vector.Y >= Y + 2f && vector.X < Right - 2f && vector.Y < Bottom - 2f)
                    {
                        mtexture.DrawCentered(vector, color);
                    }
                }
                if (whiteFill > 0f)
                {
                    Draw.Rect(X, Y, Width, Height * whiteHeight, Color.White * whiteFill);
                }
                WobbleLine(new Vector2(X, Y), new Vector2(X + Width, Y), 0f);
                WobbleLine(new Vector2(X + Width, Y), new Vector2(X + Width, Y + Height), 0.7f);
                WobbleLine(new Vector2(X + Width, Y + Height), new Vector2(X, Y + Height), 1.5f);
                WobbleLine(new Vector2(X, Y + Height), new Vector2(X, Y), 2.5f);
                Draw.Rect(new Vector2(X, Y), 2f, 2f, playerHasDreamDash ? DreamBlock.activeLineColor : DreamBlock.disabledLineColor);
                Draw.Rect(new Vector2(X + Width - 2f, Y), 2f, 2f, playerHasDreamDash ? DreamBlock.activeLineColor : DreamBlock.disabledLineColor);
                Draw.Rect(new Vector2(X, Y + Height - 2f), 2f, 2f, playerHasDreamDash ? DreamBlock.activeLineColor : DreamBlock.disabledLineColor);
                Draw.Rect(new Vector2(X + Width - 2f, Y + Height - 2f), 2f, 2f, playerHasDreamDash ? DreamBlock.activeLineColor : DreamBlock.disabledLineColor);
            }
            base.Render();
        }

        private void WobbleLine(Vector2 vector21, Vector2 vector22, float v)
        {
            Draw.Line(vector21, vector22, playerHasDreamDash ? activeLineColor : disabledLineColor);
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
                Level level = SceneAs<Level>();

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
                ref var has = ref
                ReverseHelperModule.playerHasDreamDashBetter(this);
                has = !has;

                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                level.Flash(Color.White * 0.15f, true);
                Celeste.Freeze(0.05f);
                cooldownTimer = 1f;
                DreamToggleListener.ImmediateUpdate(Scene);
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
        private bool playerHasDreamDash;
        private float animTimer;
        private int whiteHeight;
        private float whiteFill;
    }
}