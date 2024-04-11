using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.ReverseHelper.Libraries
{
    internal static class EntityDataExt
    {
        public static bool? OptionalBool(this EntityData data, string key, bool? def = null)
        {
            if (data.Values != null && data.Values.TryGetValue(key, out var value))
            {
                if (value is bool)
                {
                    return (bool)value;
                }

                if (bool.TryParse(value.ToString(), out var result))
                {
                    return result;
                }

                return null;
            }

            return def;
        }
        public static Color HexaColor(this EntityData data, string key, Color def = default)
        {
            def = Calc.HexToColorWithAlpha(data.Attr(key));
            return def;
        }
    }
}
