using System.Runtime.CompilerServices;
using Celeste.Mod.Backdrops;
using Celeste.Mod.Helpers;
using Celeste.Mod.ReverseHelper.Libraries;
using Celeste.Mod.ReverseHelper.SourceGen.Loader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod;

namespace Celeste.Mod.ReverseHelper.Backdrops;

public abstract class HiresBackgroundBackdrop : Backdrop
{
    public override void Render(Scene scene)
    {
        base.Render(scene);
        if (scene is not Level level)
        {
            return;
        }

        var device = Draw.SpriteBatch.GraphicsDevice;

        var rt = device.renderTargetBindings.Where(a => a.RenderTarget is { }).ToArray();
        var r0 = rt[0].RenderTarget as RenderTarget2D;
        if (r0 == null)
        {
            return;
        }

        var (width, height) = HiresHelper.WidthHeight(Engine.Viewport);
        var obs = HiresHelper.initialize(ref HiresHelper.buffer, width, height);
        device.SetRenderTarget(obs);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap,
            DepthStencilState.Default, RasterizerState.CullNone);
        Draw.SpriteBatch.Draw(r0, obs.Bounds, r0.Bounds, Color.White);
        if (BlendState != BlendState.AlphaBlend)
        {
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState, SamplerState.PointWrap,
                DepthStencilState.Default, RasterizerState.CullNone);
        }

        ParallaxRender(level);
        Draw.SpriteBatch.End();
        device.SetRenderTargets(rt);
        device.Clear(Color.Transparent);
    }

    public RenderRequest request;
    public BlendState BlendState = BlendState.AlphaBlend;

    public override void BeforeRender(Scene scene)
    {
        base.BeforeRender(scene);
        if (scene is not Level level)
        {
            return;
        }

        request.FillSource(level);
        var (w, h) = HiresHelper.WidthHeight(Engine.Viewport);
        var pad = new Vector2(level.ScreenPadding, level.ScreenPadding * 0.5625f) / 320f * Engine.ViewWidth;
        request.RealDestinationPosition = pad;
        request.DestinationSize = new Vector2(w, h) - pad * 2;
    }

    internal abstract void ParallaxRender(Level scene);

    [Load]
    internal static void Load()
    {
        IL.Celeste.Level.Render += LevelOnRender;
    }

    [MakeFaster]
    [ThrowDeferred]
    private static void LevelOnRender(ILContext il)
    {
        ILCursor ic = new(il);
        var v = new VariableDefinition(il.Import(typeof(Matrix)));
        il.Body.Variables.Add(v);
        ic.EmitStaticLambda(() =>
        {
            var buf = HiresHelper.buffer;
            if (buf is { })
            {
                Engine.Instance.GraphicsDevice.SetRenderTarget(buf);
                Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            }
        }, "InitBuf");
        ic.GotoNextBestFit(MoveType.After,
            i => i.MatchLdnull(),
            i => i.MatchCallOrCallvirt<GraphicsDevice>("SetRenderTarget"));
        ic.GotoNextBestFit(MoveType.After,
            i => i.MatchCallOrCallvirt<Color>("get_Black"),
            i => i.MatchCallOrCallvirt<GraphicsDevice>("Clear"));
        ic.GotoNextBestFit(MoveType.Before, i => i.MatchCallOrCallvirt<SpriteBatch>("Draw"));
        ic.GotoPrev(MoveType.Before, i => i.MatchCallOrCallvirt<SpriteBatch>("Begin"));
        ic.EmitDup();
        ic.EmitStloc(v);
        ic.Index++;
        ic.EmitLdloc(v);
        ic.EmitLdarg0();
        ic.EmitStaticLambda((Matrix m, Level level) =>
        {
            var buf = HiresHelper.buffer;
            if (buf is null || buf.IsDisposed)
            {
                return;
            }

            // m.Decompose(out var scale, ...);
            Vector2 scale = default;
            float num1 = Math.Sign(m.M11 * m.M12 * m.M13 * m.M14) < 0 ? -1f : 1f;
            float num2 = Math.Sign(m.M21 * m.M22 * m.M23 * m.M24) < 0 ? -1f : 1f;

            scale.X = num1 * (float)Math.Sqrt(m.M11 * m.M11 + m.M12 * m.M12 + m.M13 * m.M13);
            scale.Y = num2 * (float)Math.Sqrt(m.M21 * m.M21 + m.M22 * m.M22 + m.M23 * m.M23);

            if (MathHelper.WithinEpsilon(scale.X, 0.0f) || MathHelper.WithinEpsilon(scale.Y, 0.0f))
            {
                return;
            }

            Rectangle rect;

            var pad = new Vector2(level.ScreenPadding, level.ScreenPadding * 0.5625f);
            var pad2 = pad / 320f * Engine.ViewWidth;
            var (w, h) = HiresHelper.WidthHeight(Engine.Viewport);
            var bound = new Rectangle(0, 0, (int)(w - 2 * pad2.X), (int)(h - 2 * pad2.Y));
            try
            {
                var width = Convert.ToInt32(Engine.ViewWidth / scale.X);
                var height = Convert.ToInt32(Engine.ViewHeight / scale.Y);
                rect = new Rectangle((int)pad.X, (int)pad.Y, (int)(width - pad.X * 2), (int)(height - pad.Y * 2));
            }
            catch
            {
                return;
            }


            Draw.SpriteBatch.Draw(buf, rect, bound, Color.White);
        }, "RenderHiresBackground");
    }

    [Unload]
    internal static void Unload()
    {
        IL.Celeste.Level.Render -= LevelOnRender;
    }

    public HiresBackgroundBackdrop()
    {
        UseSpritebatch = false;
    }
}

public struct RenderRequest
{
    public Vector2 OffsetBase;
    public Vector2 SourcePosition;
    public Vector2 SourceSize;
    public Vector2 RealDestinationPosition;
    public Vector2 DestinationPosition => new();
    public Vector2 DestinationSize;

    public void FillSource(Level level)
    {
        var camera = level.Camera;
        OffsetBase = camera.Position;

        SourcePosition = new(camera.Left, camera.Top);
        SourceSize = new Vector2(camera.Right, camera.Bottom) - SourcePosition;
    }
}

[CustomBackdrop("ReverseHelper/YetAnotherHiresBackgroundParallax")]
public sealed class ParallaxHiresExBg : HiresBackgroundBackdrop
{
    public MTexture Texture;

    public bool DoFadeIn;

    public float Alpha = 1f;

    public float FadeIn = 1f;

    public Vector2 Scale;

    public ParallaxHiresExBg(BinaryPacker.Element elem)
    {
        string id = elem.Attr("texture");
        string text = elem.Attr("atlas", "game");
        Texture = text == "game" && GFX.Game.Has(id) ? GFX.Game[id] :
            text == "gui" && GFX.Gui.Has(id) ? GFX.Gui[id] :
            GFX.Misc[id];

        if (elem.Attr("blendmode", "alphablend").Equals("additive", StringComparison.InvariantCultureIgnoreCase))
        {
            BlendState = BlendState.Additive;
        }

        DoFadeIn = bool.Parse(elem.Attr("fadeIn", "false"));

        Scale = new Vector2(elem.AttrFloat("scaleX", 1), elem.AttrFloat("scaleY", 1));
    }

    internal override void ParallaxRender(Level scene)
    {
        var cameral = request.OffsetBase.Floor();
        Vector2 position = (Position - cameral * Scroll).Floor();
        float num = FadeIn * Alpha * FadeAlphaMultiplier;
        if (FadeX != null)
        {
            num *= FadeX.Value(cameral.X + 160f);
        }

        if (FadeY != null)
        {
            num *= FadeY.Value(cameral.Y + 90f);
        }

        Color color = Color;
        if (num < 1f)
        {
            color *= num;
        }

        if (color.A <= 1)
        {
            return;
        }

        var textureWidth = Texture.Width * Scale.X / 6;
        var textureHeight = Texture.Height * Scale.Y / 6;
        if (LoopX)
        {
            position.X = (position.X % textureWidth - textureWidth) % textureWidth;
        }

        if (LoopY)
        {
            position.Y = (position.Y % textureHeight - textureHeight) % textureHeight;
        }

        SpriteEffects spriteEffects = SpriteEffects.None;
        if (FlipX)
        {
            spriteEffects |= SpriteEffects.FlipHorizontally;
        }

        if (FlipY)
        {
            spriteEffects |= SpriteEffects.FlipVertically;
        }

        var srcpos = request.SourcePosition - cameral;
        var viewable = request.SourceSize + srcpos;
        Vector2 scaler = request.DestinationSize / request.SourceSize;
        Vector2 translater = request.DestinationPosition - srcpos * scaler;
        if (Texture.IsPacked)
        {
            for (float x = position.X; x < viewable.X; x += textureWidth)
            {
                for (float y = position.Y; y < viewable.Y; y += textureHeight)
                {
                    Texture.Draw(new Vector2(x, y) * scaler + translater, Vector2.Zero, color, scaler * Scale / 6, 0f,
                        spriteEffects);
                    if (!LoopY)
                    {
                        break;
                    }
                }

                if (!LoopX)
                {
                    break;
                }
            }
        }
        else
        {
            int num2 = (int)(LoopX ? Math.Ceiling(viewable.X - position.X) : textureWidth);
            int num3 = (int)(LoopY ? Math.Ceiling(viewable.Y - position.Y) : textureHeight);
            Rectangle value = new Rectangle(FlipX ? -num2 : 0, FlipY ? -num3 : 0, num2, num3);
            float scaleFix = Texture.ScaleFix;
            Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, position * scaler + translater, value, color, 0f,
                -Texture.DrawOffset / scaleFix * scaler, scaleFix * scaler * Scale, spriteEffects, 0f);
        }
    }

    public override void Update(Scene scene)
    {
        if (scene is not Level level)
        {
            return;
        }

        base.Update(scene);
        Position += Speed * Engine.DeltaTime;
        Position += WindMultiplier * level.Wind * Engine.DeltaTime;
        if (DoFadeIn)
        {
            FadeIn = Calc.Approach(FadeIn, Visible ? 1 : 0, Engine.DeltaTime);
        }
        else
        {
            FadeIn = Visible ? 1 : 0;
        }
    }
}
