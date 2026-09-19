using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod;

namespace Celeste.Mod.ReverseHelper.Libraries;

public class RuntimeVirtualTexture(string name, Texture2D texture, Color color)
    : VirtualTexture(name, texture.Width, texture.Height, color)
{
    [PostInitialize]
    void then(Texture2D texture)
    {
        Texture_Safe = texture;
    }
    
    public override void Reload()
    {
    }

    public void Next(Texture2D next)
    {
        Texture_Safe = next;
        Width = Texture.Width;
        Height = Texture.Height;
    }
}
