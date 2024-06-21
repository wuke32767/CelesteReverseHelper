using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [CustomEntity("ReverseHelper/TileDreamBlock")]
    [TrackedAs(typeof(DreamBlock), true)]
    public class TilesetDreamBlock : DreamBlock
    {
        public char tiletype;
        public Color lineColor;
        public Color fillColor;
        public Color linecolorDeact;
        public Color fillColorDeact;
        public bool blendIn;

        public bool bgtex;
        public bool fgmod;
        public bool bgmod;
        public TilesetDreamBlock(Vector2 position, float width, float height, char tiletype, bool blendIn,
            Color line, Color block, Color linede, Color fillde,
            Vector2? Node, bool fastmov, bool oneuse, bool below, bool bgt, bool fgc, bool bgc)
            : base(position, width, height, Node, fastmov, oneuse, below)
        {
            this.tiletype = tiletype;
            this.blendIn = blendIn;
            lineColor = line;
            fillColor = block;
            linecolorDeact = linede;
            fillColorDeact = fillde;
            bgtex = bgt;
            fgmod = fgc;
            bgmod = bgc;
            void setCollidable(bool cur)
            {
                Collidable = cur ? bgmod : fgmod;
            }
            var col = ReverseHelperExtern.BGswitch.Interop.GetBGModeListener?.Invoke(setCollidable);
            if (col is not null)
            {
                setCollidable(ReverseHelperExtern.BGswitch.Interop.IsBGMode?.Invoke() ?? false);
                Add(col);
            }
        }
        public TilesetDreamBlock(EntityData e, Vector2 offset)
            : this(e.Position + offset, e.Width, e.Height, e.Char("tiles", '3'), e.Bool("blendIn"),
                  e.HexaColor("lineColor"), e.HexaColor("fillColor"), e.HexaColor("lineColorDeactivated"), e.HexaColor("fillColorDeactivated"),
                  e.FirstNodeNullable(offset), e.Bool("fastMoving"), e.Bool("oneUse"), e.Bool("below"),
                  e.Bool("bgAppearance", false), e.Bool("fgCollidable", true), e.Bool("bgCollidable", true))
        {
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            TileGrid tileGrid;
            if (!blendIn)
            {
                tileGrid = GFX.FGAutotiler.GenerateBox(tiletype, (int)Width / 8, (int)Height / 8).TileGrid;
                Add(new LightOcclude(1f));
            }
            else
            {
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int)(X / 8f) - tileBounds.Left;
                int y = (int)(Y / 8f) - tileBounds.Top;
                int tilesX = (int)Width / 8;
                int tilesY = (int)Height / 8;
                if (bgtex)
                {
                    tileGrid = GFX.BGAutotiler.GenerateOverlay(tiletype, x, y, tilesX, tilesY, solidsData).TileGrid;
                }
                else
                {
                    tileGrid = GFX.FGAutotiler.GenerateOverlay(tiletype, x, y, tilesX, tilesY, solidsData).TileGrid;
                }
                Add(new EffectCutout());
                //Depth = -10501;
            }
            Add(tileGrid);
            Add(new TileInterceptor(tileGrid, true));
            Setup();
        }
        public new void Setup()
        {
            base.playerHasDreamDash = playerHasDreamDash;
            particles = new DreamParticle[(int)(base.Width / 8f * (base.Height / 8f) * 0.7f)];
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Position = new Vector2(Calc.Random.NextFloat(base.Width), Calc.Random.NextFloat(base.Height));
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


        public new bool playerHasDreamDash
        {
            get => DreamBlockConfigurer.dreamblock_enabled(this);
        }
        public override void Render()
        {
            Components.Render();

            Vector2 shake = new(0, 0);
            Camera camera = SceneAs<Level>().Camera;
            if (Right < camera.Left || Left > camera.Right || Bottom < camera.Top || Top > camera.Bottom)
            {
                return;
            }

            Draw.Rect(shake.X + X, shake.Y + Y, Width, Height, playerHasDreamDash ? fillColor : fillColorDeact);
            Vector2 position = SceneAs<Level>().Camera.Position;
            for (int i = 0; i < particles.Length; i++)
            {
                int layer = particles[i].Layer;
                Vector2 position2 = particles[i].Position;
                position2 += position * (0.3f + 0.25f * layer);
                position2 = PutInside(position2);
                Color color = particles[i].Color(this);
                MTexture mTexture;
                switch (layer)
                {
                    case 0:
                        {
                            int num2 = (int)((particles[i].TimeOffset * 4f + animTimer) % 4f);
                            mTexture = particleTextures[3 - num2];
                            break;
                        }
                    case 1:
                        {
                            int num = (int)((particles[i].TimeOffset * 2f + animTimer) % 2f);
                            mTexture = particleTextures[1 + num];
                            break;
                        }
                    default:
                        mTexture = particleTextures[2];
                        break;
                }

                if (position2.X >= X + 2f && position2.Y >= Y + 2f && position2.X < Right - 2f && position2.Y < Bottom - 2f)
                {
                    mTexture.DrawCentered(position2 + shake, color);
                }
            }

            //if (whiteFill > 0f)
            //{
            //    Draw.Rect(X + shake.X, Y + shake.Y, Width, Height * whiteHeight, Color.White * whiteFill);
            //}

            WobbleLine(new Vector2(X, Y), new Vector2(X + Width, Y), 0f);
            WobbleLine(new Vector2(X + Width, Y), new Vector2(X + Width, Y + Height), 0.7f);
            WobbleLine(new Vector2(X + Width, Y + Height), new Vector2(X, Y + Height), 1.5f);
            WobbleLine(new Vector2(X, Y + Height), new Vector2(X, Y), 2.5f);
            Draw.Rect(shake + new Vector2(X, Y), 2f, 2f, playerHasDreamDash ? lineColor : linecolorDeact);
            Draw.Rect(shake + new Vector2(X + Width - 2f, Y), 2f, 2f, playerHasDreamDash ? lineColor : linecolorDeact);
            Draw.Rect(shake + new Vector2(X, Y + Height - 2f), 2f, 2f, playerHasDreamDash ? lineColor : linecolorDeact);
            Draw.Rect(shake + new Vector2(X + Width - 2f, Y + Height - 2f), 2f, 2f, playerHasDreamDash ? lineColor : linecolorDeact);
        }
        public new struct DreamParticle
        {
            public Vector2 Position;

            public int Layer;

            public Color Color(Entity td)
            {
                return DreamBlockConfigurer.dreamblock_enabled(td) ? Colora : Colord;
            }

            public Color Colord;
            public Color Colora;


            public float TimeOffset;
        }
        public new float animTimer;
        //public static FieldInfo particleTextures_refl = typeof(DreamBlock).GetField("particleTextures", BindingFlags.Instance | BindingFlags.NonPublic);
        //public MTexture[] particleTextures { get => (MTexture[])particleTextures_refl.GetValue(this); }

        public new DreamParticle[] particles = null!;
        private new void WobbleLine(Vector2 from, Vector2 to, float offset)
        {
            float num = (to - from).Length();
            Vector2 vector = Vector2.Normalize(to - from);
            Vector2 vector2 = new Vector2(vector.Y, 0f - vector.X) / 2f;
            //Color color = (linecolor);
            //Color color2 = (Color.White);
            //if (whiteFill > 0f)
            //{
            //    color = Color.Lerp(color, Color.White, whiteFill);
            //    color2 = Color.Lerp(color2, Color.White, whiteFill);
            //}

            float num2 = 0f;
            int num3 = 16;
            for (int i = 2; i < num - 2f; i += num3)
            {
                float num4 = Lerp(LineAmplitude(wobbleFrom + offset, i), LineAmplitude(wobbleTo + offset, i), wobbleEase);
                if (i + num3 >= num)
                {
                    num4 = 0f;
                }

                float num5 = Math.Min(num3, num - 2f - i);
                Vector2 vector3 = from + vector * i + vector2 * num2;
                Vector2 vector4 = from + vector * (i + num5) + vector2 * num4;
                //Draw.Line(vector3 - vector2, vector4 - vector2, color2);
                //Draw.Line(vector3 - vector2 * 2f, vector4 - vector2 * 2f, color2);
                Draw.Line(vector3, vector4, playerHasDreamDash ? lineColor : linecolorDeact);
                num2 = num4;
            }
        }
        private new static float Lerp(float a, float b, float percent)
        {
            return a + (b - a) * percent;
        }
        private static new float LineAmplitude(float seed, float index)
        {
            return (float)(Math.Sin((double)(seed + index / 16f) + Math.Sin(seed * 2f + index / 32f) * 6.2831854820251465) + 1.0) * 1.5f;
        }

        private new float wobbleFrom = Calc.Random.NextFloat((float)Math.PI * 2f);

        private new float wobbleTo = Calc.Random.NextFloat((float)Math.PI * 2f);

        private new float wobbleEase;

        public override void Update()
        {
            base.Update();
            if (playerHasDreamDash)
            {
                animTimer += 6f * Engine.DeltaTime;
                wobbleEase += Engine.DeltaTime * 2f;
                if (wobbleEase > 1f)
                {
                    wobbleEase = 0f;
                    wobbleFrom = wobbleTo;
                    wobbleTo = Calc.Random.NextFloat((float)Math.PI * 2f);
                }

                SurfaceSoundIndex = 12;
            }
            else
            {
                SurfaceSoundIndex = 11;
            }

        }

        new Vector2 PutInside(Vector2 pos)
        {
            if (pos.X > base.Right)
            {
                pos.X -= (float)Math.Ceiling((pos.X - base.Right) / base.Width) * base.Width;
            }
            else if (pos.X < base.Left)
            {
                pos.X += (float)Math.Ceiling((base.Left - pos.X) / base.Width) * base.Width;
            }

            if (pos.Y > base.Bottom)
            {
                pos.Y -= (float)Math.Ceiling((pos.Y - base.Bottom) / base.Height) * base.Height;
            }
            else if (pos.Y < base.Top)
            {
                pos.Y += (float)Math.Ceiling((base.Top - pos.Y) / base.Height) * base.Height;
            }

            return pos;
        }
    }
}