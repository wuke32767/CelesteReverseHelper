using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.ReverseHelper.Libraries
{
    internal static class HexAColor
    {
        public static Color HexaColor(this EntityData data, string key, Color def = default)
        {
            def = Calc.HexToColorWithAlpha(data.Attr(key));
            return def;
        }
    }
}
