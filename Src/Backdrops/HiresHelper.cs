using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.ReverseHelper.Backdrops;

public static class HiresHelper
{
    internal static RenderTarget2D initialize(ref RenderTarget2D? buffer, int width, int height)
    {
        if (buffer == null || buffer.IsDisposed || buffer.Width < width || buffer.Height < height)
        {
            buffer?.Dispose();
            buffer = new RenderTarget2D(Engine.Graphics.GraphicsDevice, width, height);
        }

        return buffer;
    }
    
    internal static float GameScale(Level level)
    {
        return Engine.ViewWidth / (float)level.Camera.Viewport.Width;
    }
    
    internal static float GetScale(Level level)
    {
        return 1920f / level.Camera.Viewport.Width;
    }
    
    public static RenderTarget2D GetBuffer(int width, int height)
    {
        var obs = initialize(ref buffer, width, height);
        return obs;
    }

    public static (int x, int y) WidthHeight(Viewport vp) => new(vp.Width, vp.Height);
    public static RenderTarget2D? buffer;
}
