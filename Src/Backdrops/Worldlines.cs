using Celeste.Mod.Backdrops;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Celeste.Mod.Helpers;
using Celeste.Mod.ReverseHelper.Libraries;
using Celeste.Mod.ReverseHelper.SourceGen.Loader;
using MonoMod;

namespace Celeste.Mod.ReverseHelper.Backdrops;

[CustomBackdrop("ReverseHelper/ThetaAndParalldoxsOnWorldlines/Sea = Load")]
public class WorldlinesConstructor
{
    public static Backdrop Load(BinaryPacker.Element elem)
    {
        if (elem.AttrBool("hires"))
        {
            return new HiresWorldlinesBackdrop(elem);
        }

        return new LowresWorldlinesBackdrop(elem);
    }
}

public struct WorldlinesCore
{
    //private float Alpha => FadeAlphaMultiplier * Math.Clamp(Color.A / 255f, 0, 1);
    internal RenderTarget2D? buffer;

    private const int designedCount = 16;
    private const float designedWidth = 1920;
    private const float designedHeight = 1080;
    private const float padding = 600;
    private const float expansion = 300;
    private const float finalWidth = designedWidth + padding * 2 + expansion * 2;
    private const float finalHeight = designedHeight + padding * 2 + expansion * 2;
    private const float density = designedCount / designedWidth / designedHeight;

    public WorldlinesCore(Chapters ch, float timeScale, float scrollX, float scrollY)
    {
        ch1 = ch;
        this.timeScale = timeScale;
        this.scrollX = scrollX;
        this.scrollY = scrollY;
    }

    internal RenderTarget2D GetTarget(int width, int height)
    {
        if (buffer is not { } buf || buf.Width < width || buf.Height < height)
        {
            buffer = new(Engine.Graphics.GraphicsDevice, width, height);
        }

        return buffer;
    }

    [InlineArray(4)]
    private struct BigVertex
    {
        public MyVertex Allin;

        public Vector2 Position => this[0].Position;
    }

    public bool Hires => hires;

    public enum Chapters
    {
        Parallel,
        Interference,
        Observation,
        Emergence,
        Entanglement,
        Overflow,
    }

    private string tostr(Chapters c) => c switch
    {
        Chapters.Parallel => "Parallel",
        Chapters.Interference => "Interference",
        Chapters.Observation => "Observation",
        Chapters.Emergence => "Emergence",
        Chapters.Entanglement => "Entanglement",
        Chapters.Overflow => "Overflow",
        _ => "",
    };

    public WorldlinesCore(BinaryPacker.Element child)
        : this(Enum.TryParse<Chapters>(child.Attr("chapter"), out var ch) ? ch : Chapters.Overflow,
            child.AttrFloat("timeScale"),
            child.AttrFloat("scrollX", 0), child.AttrFloat("scrollY", 0))
    {
    }

    private static BlendState Maxwell = new()
    {
        AlphaBlendFunction = BlendFunction.Max,
        ColorBlendFunction = BlendFunction.Max,
        ColorSourceBlend = Blend.SourceColor,
        AlphaSourceBlend = Blend.SourceAlpha,
        ColorDestinationBlend = Blend.DestinationColor,
        AlphaDestinationBlend = Blend.DestinationAlpha,
    };

    private static Lazy<Effect> effect = new(() => new(Engine.Graphics.GraphicsDevice,
        Everest.Content.Get("ReverseHelper:/Effects/ReverseHelper/paralldox.fxb").Data));

    private readonly float rand = Random.Shared.NextSingle();
    private float time = 0;

    private static WhyColorsWhen[] normalarray = new WhyColorsWhen[6];

    [StructLayout(LayoutKind.Sequential)]
    private struct MyVertex : IVertexType
    {
        public Vector2 Position;
        public Vector2 Offset;
        public VertexDeclaration VertexDeclaration => vertexDeclaration;

        public static VertexDeclaration vertexDeclaration =
            new(new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.Position, 1));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WhyColorsWhen() : IVertexType
    {
        public Vector2 Position;
        public Vector2 Down;
        public VertexDeclaration VertexDeclaration => vertexDeclaration;

        public static VertexDeclaration vertexDeclaration =
            new(new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.Position, 1));
    }

    private struct ObservationProvider()
    {
        [PostInitialize]
        private void Init()
        {
            reserve(designedCount * 2);
        }

        public static int NextNormal(float lambda)
        {
            double u1 = Random.Shared.NextDouble();
            if (u1 == 0) u1 = double.Epsilon;
            double u2 = Random.Shared.NextDouble();
            double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            double sample = lambda + Math.Sqrt(lambda) * z;
            int result = (int)Math.Round(sample);
            return Math.Max(0, result);
        }

        public static int NextPoisson(float lambda)
        {
            [ForceTailRec]
            static int Inner(double l, int k = 0, double p = 1)
            {
                if (p < l)
                {
                    return Math.Max(k - 1, 0);
                }

                return Inner(l, k + 1, p * Random.Shared.NextDouble());
            }

            if (lambda > 30)
            {
                return NextNormal(lambda);
            }

            return Inner(Math.Exp(-lambda));
        }

        public void ApplyNew(Rectangle at)
        {
            var nw = Rectangle.Intersect(at, Ranges);
            if (nw == default)
            {
                nw = new(at.X, at.Y, 0, 0);
            }

            RemoveExcept(nw);
            Subtract(at, nw);
            Ranges = at;
        }

        private void Subtract(Rectangle A, Rectangle B)
        {
            int aRight = A.X + A.Width;
            int aBottom = A.Y + A.Height;
            int bRight = B.X + B.Width;
            int bBottom = B.Y + B.Height;

            if (B.Y > A.Y)
            {
                Expand(new Rectangle(A.X, A.Y, A.Width, B.Y - A.Y));
            }

            if (bBottom < aBottom)
            {
                Expand(new Rectangle(A.X, bBottom, A.Width, aBottom - bBottom));
            }

            if (B.X > A.X)
            {
                Expand(new Rectangle(A.X, B.Y, B.X - A.X, B.Height));
            }

            if (bRight < aRight)
            {
                Expand(new Rectangle(bRight, B.Y, aRight - bRight, B.Height));
            }
        }

        private void Expand(Rectangle nw)
        {
            var sur = nw.Width * nw.Height;
            var cnt = NextPoisson(sur * density);
            reserve(cnt + Current);
            for (int i = 0; i < cnt; i++)
            {
                var sp = Vertices.AsSpan((i + Current) * 4, 4);
                var x = Random.Shared.NextSingle() * nw.Width + nw.X;
                var y = Random.Shared.NextSingle() * nw.Height + nw.Y;
                sp[0] = new() { Position = new(x, y), Offset = new(1, 1) };
                sp[1] = new() { Position = new(x, y), Offset = new(-1, 1) };
                sp[2] = new() { Position = new(x, y), Offset = new(1, -1) };
                sp[3] = new() { Position = new(x, y), Offset = new(-1, -1) };
            }

            Current += cnt;
        }

        private void RemoveExcept(Rectangle nw)
        {
            var arr = Vert2;
            int i = 0, end = Current;
            while (i < end)
            {
                ref var cur = ref arr[i];
                if (!nw.Contains(cur.Position.ToPoint()))
                {
                    end--;
                    (cur, arr[end]) = (arr[end], cur);
                }
                else
                {
                    i++;
                }
            }

            Current = end;
        }

        private void reserve(int cnt)
        {
            Vertices ??= [];
            Indexes ??= [];
            var t = Vertices.Length / 4;
            if (t < cnt)
            {
                cnt = Math.Max(cnt, t * 2);
                Array.Resize(ref Vertices, cnt * 4);
                Array.Resize(ref Indexes, cnt * 6);
                for (int i = t; i < cnt; i++)
                {
                    var sp = Indexes.AsSpan(i * 6, 6);
                    var f = i * 4;
                    ((Span<int>)[f, f + 1, f + 2, f + 1, f + 2, f + 3]).CopyTo(sp);
                }
            }
        }

        public MyVertex[] Vertices = [];
        public int[] Indexes = [];

        public Span<BigVertex> Vert2 => MemoryMarshal.Cast<MyVertex, BigVertex>(Vertices.AsSpan())[..Current];

        public int Current = 0;
        public Rectangle Ranges;
    }

    private bool inited = false;
    private ObservationProvider prov = default;
    private Vector2 basicPoint;
    private readonly WorldlinesCore.Chapters ch1;
    private readonly float timeScale;
    private readonly float scrollX;
    private readonly float scrollY;


    const bool hires = true;
    private Vector2 obsOffset => new Vector2(30, 10) * time;
    private const float length = 40;

    public void Update(Scene scene)
    {
        var level = (Level)scene;
        time += Engine.DeltaTime * timeScale;
        if (!inited)
        {
            inited = true;
            basicPoint = level.Bounds.Center.ToVector2();
            if (IsObservation)
            {
                prov = new();
            }
        }
    }

    private bool IsObservation => ch1 == Chapters.Observation;

    public unsafe void BeforeRender(
        Level level, delegate*<int, int, RenderTarget2D> getBuffer,
        RenderRequest request)
    {
        var width = (int)request.DestinationSize.X;
        var height = (int)request.DestinationSize.Y;
        RenderTarget2D buf = GetTarget(width, height);
        var eff = effect.Value;
        var reso = 1; //designedWidth / width;
        var tech = eff.Techniques[tostr(ch1)];
        eff.CurrentTechnique = tech;
        eff.Parameters["randomizer"].SetValue(rand);
        eff.Parameters["time"].SetValue(time);

        var gd = Engine.Graphics.GraphicsDevice;

        // i'm lazy to set up all state myself
        Draw.SpriteBatch.Begin(SpriteSortMode.Immediate, Maxwell, SamplerState.PointWrap, DepthStencilState.Default,
            RasterizerState.CullNone, effect.Value);
        if (IsObservation)
        {
            var size = new Vector2(buf.Width, buf.Height);
            var ratio = request.DestinationSize / size;
            var actual = ratio * 2 - new Vector2(1, 1);
            NewFunction(actual, normalarray, new Vector2(0, 0), new Vector2(1) / ratio);
            var cen = request.SourcePosition * 6 * new Vector2(scrollX, scrollY) + obsOffset + request.SourceSize * 3;
            // var cen = level.Camera.Position * 6 * new Vector2(scrollX, scrollY) + obsOffset +
            //     new Vector2(designedWidth / 2, designedHeight / 2);
            var exp = new Vector2(expansion + padding) + request.SourceSize * 3;
            prov.ApplyNew(new Rectangle((int)(cen.X - exp.X), (int)(cen.Y - exp.Y),
                (int)(exp.X * 2), (int)(exp.Y * 2)));

            eff.Parameters["resoolution"].SetValue(request.DestinationSize);
            eff.Parameters["offsetOfObservation"]
                .SetValue(request.SourcePosition * 6 * new Vector2(scrollX, scrollY) + obsOffset);
            var obs = getBuffer(width, height);
            foreach (ref var node in normalarray.AsSpan())
            {
                node.Down *= request.DestinationSize / new Vector2(obs.Width, obs.Height);
            }

            gd.SetRenderTarget(obs);
            gd.Clear(Color.Transparent);
            var pass = eff.CurrentTechnique.Passes;
            var circle = pass[0];
            var sea = pass[1];

            circle.Apply();
            gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, prov.Vertices, 0, prov.Current * 4,
                prov.Indexes, 0, prov.Current * 2);
            sea.Apply();
            gd.SetRenderTarget(buf);
            gd.Clear(Color.Transparent);
            gd.Textures[0] = obs;
            gd.DrawUserPrimitives(PrimitiveType.TriangleList, normalarray, 0, 2);
        }
        else
        {
            var size = new Vector2(buf.Width, buf.Height);
            var actual = request.DestinationSize / size * 2 - new Vector2(1, 1);
            var pos = (request.SourcePosition - basicPoint) * new Vector2(scrollX, scrollY) / new Vector2(320, 180);
            var add = request.SourceSize / new Vector2(320, 180);
            NewFunction(actual, normalarray, pos, pos + add);
            gd.SetRenderTarget(buf);
            gd.Clear(Color.Transparent);
            foreach (var pass in eff.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, normalarray, 0, 2);
            }
        }

        Draw.SpriteBatch.End();

        static void NewFunction(Vector2 vector2, WhyColorsWhen[] on, Vector2 from, Vector2 range)
        {
            on[0] = new() { Position = new(-1, 1), Down = new(from.X, range.Y) };
            on[1] = new() { Position = new(-1, -vector2.Y), Down = from };
            on[2] = new() { Position = new(vector2.X, 1), Down = range };
            on[3] = new() { Position = new(vector2.X, -vector2.Y), Down = new(range.X, from.Y) };
            on[4] = on[2];
            on[5] = on[1];
        }
    }

    public void Dispose()
    {
        buffer?.Dispose();
        buffer = null;
    }
}

class HiresWorldlinesBackdrop : HiresBackgroundBackdrop
{
    WorldlinesCore core;

    private static (int x, int y) WH(Viewport vp) => new(vp.Width, vp.Height);

    public HiresWorldlinesBackdrop(BinaryPacker.Element elem)
    {
        core = new WorldlinesCore(elem);
        Scroll = default;
        Speed = default;
    }

    public override void Update(Scene scene)
    {
        core.Update(scene);
        base.Update(scene);
    }

    public override unsafe void BeforeRender(Scene scene)
    {
        base.BeforeRender(scene);
        core.BeforeRender((Level)scene, &HiresHelper.GetBuffer, request);
    }

    internal override void ParallaxRender(Level scene)
    {
        var p = request.DestinationSize.ToPoint();
        Draw.SpriteBatch.Draw(core.buffer, request.DestinationPosition, new(0, 0, p.X, p.Y), Color);
    }

    public override void Ended(Scene scene)
    {
        core.Dispose();
        base.Ended(scene);
    }
}

class LowresWorldlinesBackdrop(BinaryPacker.Element elem) : Backdrop
{
    WorldlinesCore core = new(elem);
    private static RenderTarget2D? yetanother;

    public override void Update(Scene scene)
    {
        core.Update(scene);
        base.Update(scene);
    }

    public override unsafe void BeforeRender(Scene scene)
    {
        var level = (Level)scene;
        var req = new RenderRequest();
        req.FillSource(level);
        var b = GameplayBuffers.Gameplay;
        req.DestinationSize = new(b.Width, b.Height);
        core.BeforeRender(level, &HiresHelper.GetBuffer, req);
    }

    public override void Render(Scene scene)
    {
        var level = (Level)scene;
        var (width, height) = HiresHelper.WidthHeight(level.Camera.Viewport);
        Draw.SpriteBatch.Draw(core.buffer, new Vector2(), new Rectangle(0, 0, width, height), Color);
    }

    public override void Ended(Scene scene)
    {
        core.Dispose();
        yetanother?.Dispose();
        yetanother = null;
        base.Ended(scene);
    }
}
