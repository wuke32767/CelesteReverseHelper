using Microsoft.Xna.Framework;
using System.Globalization;

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
        public static List<T> List<T>(this EntityData data, string key, Func<string, T> parser, T def = default!)
        {
            var de = data.Attr(key);
            if (!string.IsNullOrWhiteSpace(de))
            {
                try
                {
                    return de.Split(',').Select(x => parser(x.Trim())).ToList();
                }
                catch
                {
                }
            }
            return [def];
        }
        public delegate bool TryParser<T>(string s, out T val);
        // not necessary imo
        public static readonly TryParser<float> floatParse = (string s, out float v) => float.TryParse(s, CultureInfo.InvariantCulture, out v);
        public static List<T> List<T>(this EntityData data, string key, TryParser<T> parser, T def = default!)
        {
            List<T> ret = [];
            var de = data.Attr(key);
            if (!string.IsNullOrWhiteSpace(de))
            {
                foreach (var i in de.Split(','))
                {
                    if (!parser(i, out var v))
                    {
                        return [def];
                    }
                    ret.Add(v);
                }
            }
            if (ret.Count == 0)
            {
                ret.Add(def);
            }
            return ret;
        }
        public static Color HexaColor(this EntityData data, string key, Color def = default)
        {
            def = Calc.HexToColorWithAlpha(data.Attr(key));
            return def;
        }
    }
}
