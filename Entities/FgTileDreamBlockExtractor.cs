using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [CustomEntity("ReverseHelper/FgTileDreamBlockExtractor")]
    [Tracked(true)]
    [TrackedAs(typeof(DreamBlock), true)]
    public class FgTileDreamBlockExtractor : DreamBlock
    {
        private static readonly BlendState DreamParticleBlend = new()
        {
            ColorSourceBlend = Blend.DestinationAlpha,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            AlphaSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add,
        };
        private static readonly BlendState ClearBlend = new()
        {
            ColorSourceBlend = Blend.Zero,
            ColorDestinationBlend = Blend.DestinationAlpha,
            AlphaSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add,
        };

        ~FgTileDreamBlockExtractor()
        {
            VirtualRenderTarget.Target.Dispose();
            VirtualRenderTarget.Target = null;
        }
        List<(Vector2 from, Vector2 to)> WobbleList = [];
        List<Vector2> CornerList = [];
        VirtualRenderTarget VirtualRenderTarget;
        Rectangle intersection;

        public Color lineColor;
        public Color fillColor;
        public Color linecolorDeact;
        public Color fillColorDeact;

        public FgTileDreamBlockExtractor(Vector2 position, int width, int height, Color line, Color block,
            Color linede, Color fillde) : base(position, width, height, null, false, false, false)
        {
            lineColor = line;
            fillColor = block;
            linecolorDeact = linede;
            fillColorDeact = fillde;
            DreamBlockConfig.Get(this).HighPriority();
            SurfaceSoundPriority = 11;
            VirtualRenderTarget = new VirtualRenderTarget("ReverseHelper_TileDreamBlock", Math.Min((int)Width, 320), Math.Min((int)Height, 180), 0, false, true);

        }

        public FgTileDreamBlockExtractor(EntityData e, Vector2 offset)
            : this(e.Position + offset, e.Width, e.Height,
                e.HexaColor("lineColor"), e.HexaColor("fillColor"),
                e.HexaColor("lineColorDeactivated"), e.HexaColor("fillColorDeactivated"))
        {
        }


        public override void Awake(Scene _scene)
        {
            base.Awake(_scene);
            var scene = SceneAs<Level>();
            Rectangle tileBounds = scene.Session.MapData.TileBounds;
            var tiletex = scene.SolidTiles.Tiles.Tiles;
            int x = (int)(X / 8f) - tileBounds.Left;
            int y = (int)(Y / 8f) - tileBounds.Top;
            int tilesX = (int)Width / 8;
            int tilesY = (int)Height / 8;
            var collidemap = new VirtualMap<bool>(tilesX, tilesY, false);
            var tg = new TileGrid(8, 8, tilesX, tilesY);
            for (int i = 0; i < tilesX; i++)
            {
                for (int j = 0; j < tilesY; j++)
                {
                    if (tiletex[x + i, y + j] != tiletex.EmptyValue)
                    {
                        collidemap[i, j] = true;
                        tg.Tiles[i, j] = tiletex[x + i, y + j];
                    }
                }
            }
            Collider = new Grid(8, 8, collidemap);
            Add(tg);

            var tilechar = scene.SolidsData;
            for (int i = 0; i < tilesX; i++)
            {
                for (int j = 0; j < tilesY; j++)
                {
                    if (tilechar[x + i, y + j] != tilechar.EmptyValue)
                    {
                        if (tilechar[x + i + 1, y + j] == tilechar.EmptyValue && tilechar[x + i, y + j + 1] == tilechar.EmptyValue)
                        {
                            CornerList.Add(new(i * 8 + 6, j * 8 + 6));
                        }
                        if (tilechar[x + i, y + j + 1] == tilechar.EmptyValue && tilechar[x + i - 1, y + j] == tilechar.EmptyValue)
                        {
                            CornerList.Add(new(i * 8 + 1, j * 8 + 6));
                        }
                        if (tilechar[x + i - 1, y + j] == tilechar.EmptyValue && tilechar[x + i, y + j - 1] == tilechar.EmptyValue)
                        {
                            CornerList.Add(new(i * 8 + 1, j * 8 + 1));
                        }
                        if (tilechar[x + i, y + j - 1] == tilechar.EmptyValue && tilechar[x + i + 1, y + j] == tilechar.EmptyValue)
                        {
                            CornerList.Add(new(i * 8 + 6, j * 8 + 1));
                        }
                        if (tilechar[x + i + 1, y + j] != tilechar.EmptyValue
                         && tilechar[x + i, y + j + 1] != tilechar.EmptyValue
                         && tilechar[x + i + 1, y + j + 1] == tilechar.EmptyValue)
                        {
                            CornerList.Add(new(i * 8 + 7, j * 8 + 7));
                        }
                        if (tilechar[x + i, y + j + 1] != tilechar.EmptyValue
                         && tilechar[x + i - 1, y + j] != tilechar.EmptyValue
                         && tilechar[x + i - 1, y + j + 1] == tilechar.EmptyValue)
                        {
                            CornerList.Add(new(i * 8 + 0, j * 8 + 7));
                        }
                        if (tilechar[x + i - 1, y + j] != tilechar.EmptyValue
                         && tilechar[x + i, y + j - 1] != tilechar.EmptyValue
                         && tilechar[x + i - 1, y + j - 1] == tilechar.EmptyValue)
                        {
                            CornerList.Add(new(i * 8 + 0, j * 8 + 0));
                        }
                        if (tilechar[x + i, y + j - 1] != tilechar.EmptyValue
                         && tilechar[x + i + 1, y + j] != tilechar.EmptyValue
                         && tilechar[x + i + 1, y + j - 1] == tilechar.EmptyValue)
                        {
                            CornerList.Add(new(i * 8 + 7, j * 8 + 0));
                        }
                    }
                }
            }
            for (int i = 0; i < tilesX; i++)
            {
                Vector2? froml = null, fromr = null;
                for (int j = 0; j < tilesY; j++)
                {
                    // build vertical edges of dream block.
                    if (tilechar[x + i, y + j] != tilechar.EmptyValue && tilechar[x + i - 1, y + j] == tilechar.EmptyValue)
                    {
                        if (froml is null)
                        {
                            froml = new(i * 8, j * 8);
                        }
                    }
                    else
                    {
                        if (froml is not null)
                        {
                            WobbleList.Add((new(i * 8, j * 8), froml.Value));
                            froml = null;
                        }
                    }
                    if (tilechar[x + i, y + j] != tilechar.EmptyValue && tilechar[x + i + 1, y + j] == tilechar.EmptyValue)
                    {
                        if (fromr is null)
                        {
                            fromr = new(i * 8, j * 8);
                        }
                    }
                    else
                    {
                        if (fromr is not null)
                        {
                            WobbleList.Add((fromr.Value + new Vector2(8, 0), new(8 + i * 8, j * 8)));
                            fromr = null;
                        }
                    }
                }
                if (froml is not null)
                {
                    WobbleList.Add((new(i * 8, tilesY * 8), froml.Value));
                }
                if (fromr is not null)
                {
                    WobbleList.Add((fromr.Value + new Vector2(8, 0), new(8 + i * 8, tilesY * 8)));
                }

            }
            for (int j = 0; j < tilesY; j++)
            {
                Vector2? fromu = null, fromd = null;
                for (int i = 0; i < tilesX; i++)
                {
                    // build horizonal edges of dream block.
                    if (tilechar[x + i, y + j] != tilechar.EmptyValue && tilechar[x + i, y + j - 1] == tilechar.EmptyValue)
                    {
                        if (fromu is null)
                        {
                            fromu = new(i * 8, j * 8);
                        }
                    }
                    else
                    {
                        if (fromu is not null)
                        {
                            WobbleList.Add((fromu.Value, new(i * 8, j * 8)));
                            fromu = null;
                        }
                    }
                    if (tilechar[x + i, y + j] != tilechar.EmptyValue && tilechar[x + i, y + j + 1] == tilechar.EmptyValue)
                    {
                        if (fromd is null)
                        {
                            fromd = new(i * 8, j * 8);
                        }
                    }
                    else
                    {
                        if (fromd is not null)
                        {
                            WobbleList.Add((new(i * 8, j * 8 + 8), fromd.Value + new Vector2(0, 8)));
                            fromd = null;
                        }
                    }
                }
                if (fromu is not null)
                {
                    WobbleList.Add((fromu.Value, new(tilesX * 8, j * 8)));
                }
                if (fromd is not null)
                {
                    WobbleList.Add((new(tilesX * 8, j * 8 + 8), fromd.Value + new Vector2(0, 8)));
                }
            }

            Setup();
            Add(new BeforeRenderHook(BeforeRender));
        }
        enum _edges : byte
        {
            up = 1, left = 2, right = 4, down = 8,
        }
        TilesetDreamBlock.DreamParticle[] particles;
        public new void Setup()
        {
            particles = new TilesetDreamBlock.DreamParticle[(int)(base.Width / 8f * (base.Height / 8f) * 0.7f)];
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


        public bool playerHasDreamDash
        {
            get => ReversedDreamBlock.dreamblock_enabled(this);
        }
        private void BeforeRender()
        {
            Camera camera = SceneAs<Level>().Camera;
            Rectangle camerarect = new Rectangle((int)camera.X, (int)camera.Y, 320, 180);
            Rectangle self = new Rectangle((int)X, (int)Y, Math.Min((int)Width, 320), Math.Min((int)Height, 180));

            if (!camerarect.Intersects(self))
            {
                intersection = new(0, 0, 0, 0);
                return;
            }
            intersection = Rectangle.Intersect(camerarect, self);

            //VirtualRenderTarget ??= new VirtualRenderTarget("ReverseHelper_TileDreamBlock", Math.Min((int)Width, 320), Math.Min((int)Height, 180), 0, false, true);
            Engine.Graphics.GraphicsDevice.SetRenderTarget(VirtualRenderTarget);

            Matrix cammat = camera.Matrix;
            //seems TileGrid don't like this.
            //var translation = camera.position - Position;
            //cammat = Matrix.CreateTranslation(translation.X, translation.Y, 0) * cammat;
            var mov = camera.position - new Vector2(intersection.Left, intersection.Top);
            Position += mov;

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, cammat);
            Draw.Rect(camera.X, camera.Y, intersection.Width, intersection.Height, Color.Transparent);//Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Components.Render();
            Draw.SpriteBatch.End();


            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, DreamParticleBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, cammat);
            Vector2 shake = new(0, 0);
            Draw.Rect(shake.X + X, shake.Y + Y, Width, Height, playerHasDreamDash ? fillColor : fillColorDeact);
            Vector2 position = SceneAs<Level>().Camera.Position;
            for (int i = 0; i < particles.Length; i++)
            {
                int layer = particles[i].Layer;
                Vector2 position2 = particles[i].Position;
                position2 += position * (0.3f + 0.25f * layer);
                position2 = PutInside2(position2);
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
                if (position2.X >= 2f && position2.Y >= 2f && position2.X < Width - 2f && position2.Y < Height - 2f)
                {
                    mTexture.DrawCentered(Position + position2 + shake, color);
                }
            }
            Draw.SpriteBatch.End();

            Position -= mov;
        }

        public override void Render()
        {
            if (intersection.IsEmpty)
            {
                return;
            }
            Draw.SpriteBatch.Draw(VirtualRenderTarget, intersection, new(0, 0, intersection.Width, intersection.Height), Color.White);
            //if (whiteFill > 0f)
            //{
            //    Draw.Rect(X + shake.X, Y + shake.Y, Width, Height * whiteHeight, Color.White * whiteFill);
            //}
            float off = 0;
            foreach (var (f, t) in WobbleList)
            {
                WobbleLine(f + Position, t + Position, off += 0.75f);
            }
            //WobbleLine(new Vector2(X, Y), new Vector2(X + Width, Y), 0f);
            //WobbleLine(new Vector2(X + Width, Y), new Vector2(X + Width, Y + Height), 0.7f);
            //WobbleLine(new Vector2(X + Width, Y + Height), new Vector2(X, Y + Height), 1.5f);
            //WobbleLine(new Vector2(X, Y + Height), new Vector2(X, Y), 2.5f);
            foreach (var r in CornerList)
            {
                Draw.Rect(shake + r + Position, 1, 1, playerHasDreamDash ? lineColor : linecolorDeact);
            }
            //Draw.Rect(shake + new Vector2(X, Y), 2f, 2f, playerHasDreamDash ? lineColor : lineColor);
            //Draw.Rect(shake + new Vector2(X + Width - 2f, Y), 2f, 2f, playerHasDreamDash ? lineColor : lineColor);
            //Draw.Rect(shake + new Vector2(X, Y + Height - 2f), 2f, 2f, playerHasDreamDash ? lineColor : lineColor);
            //Draw.Rect(shake + new Vector2(X + Width - 2f, Y + Height - 2f), 2f, 2f, playerHasDreamDash ? lineColor : lineColor);
        }
        private void WobbleLine(Vector2 from, Vector2 to, float offset)
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
            for (int i = 0; i < num; i += num3)
            {
                float num4 = Lerp(LineAmplitude(wobbleFrom + offset, i), LineAmplitude(wobbleTo + offset, i), wobbleEase);
                if (i + num3 >= num)
                {
                    num4 = 0f;
                }

                float num5 = Math.Min(num3, num - i);
                Vector2 vector3 = from + vector * i + vector2 * num2;
                Vector2 vector4 = from + vector * (i + num5) + vector2 * num4;
                //Draw.Line(vector3 - vector2, vector4 - vector2, color2);
                //Draw.Line(vector3 - vector2 * 2f, vector4 - vector2 * 2f, color2);
                Draw.Line(vector3, vector4, playerHasDreamDash ? lineColor : linecolorDeact);
                num2 = num4;
            }
        }
        private float Lerp(float a, float b, float percent)
        {
            return a + (b - a) * percent;
        }
        private float LineAmplitude(float seed, float index)
        {
            return (float)(Math.Sin((double)(seed + index / 16f) + Math.Sin(seed * 2f + index / 32f) * 6.2831854820251465) + 1.0) * 1.5f;
        }

        private float wobbleFrom = Calc.Random.NextFloat((float)Math.PI * 2f);

        private float wobbleTo = Calc.Random.NextFloat((float)Math.PI * 2f);

        private float wobbleEase;

        public override void Update()
        {
            Entity_Update();
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
        [MonoMod.MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
        public void Entity_Update()
        {
            throw new NotImplementedException("How? This should be relinked.");
        }
        Vector2 PutInside2(Vector2 pos)
        {
            int i = (int)pos.X;
            i /= (int)Width;
            if (i < 0)
            {
                i = i - 1;
            }
            pos.X -= i * Width;
            int j = (int)pos.Y;
            j /= (int)Height;
            if (j < 0)
            {
                j = j - 1;
            }
            pos.Y -= j * Height;
            //if (pos.X > base.Right)
            //{
            //    pos.X -= (float)Math.Ceiling((pos.X - base.Right) / base.Width) * base.Width;
            //}
            //else if (pos.X < base.Left)
            //{
            //    pos.X += (float)Math.Ceiling((base.Left - pos.X) / base.Width) * base.Width;
            //}

            //if (pos.Y > base.Bottom)
            //{
            //    pos.Y -= (float)Math.Ceiling((pos.Y - base.Bottom) / base.Height) * base.Height;
            //}
            //else if (pos.Y < base.Top)
            //{
            //    pos.Y += (float)Math.Ceiling((base.Top - pos.Y) / base.Height) * base.Height;
            //}

            return pos;
        }

    }
}