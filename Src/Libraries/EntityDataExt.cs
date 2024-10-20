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
        public delegate bool TryParser<T>(string s, out T val);
        // not necessary imo
        public static readonly TryParser<float> floatParse = (string s, out float v) => float.TryParse(s, CultureInfo.InvariantCulture, out v);
        struct optional<T>
        {
            public optional(T value)
            {
                val = value;
                Has = true;
            }
            public optional()
            {
                Has = false;
            }
            public bool Has = false;
            public T val=default!;
            public T Value { get => Has ? val! : throw new NullReferenceException(); }
        }
        static IEnumerable<optional<T>> ListImpl<T>(EntityData data, string key, TryParser<T> parser, int len)
        {
            var de = data.Attr(key);
            foreach (var i in de.Split(','))
            {
                if(len==0)
                {
                    yield break;
                }
                if (!parser(i, out var v))
                {
                    yield return default;
                }
                else
                {
                    yield return new optional<T>(v);
                }
                len--;
            }
            while(len!=0)
            {
                yield return default;
                len--;
            }
        }
        public static IEnumerable<T> List<T>(this EntityData data, string key, TryParser<T> parser, int len, T def)
        {
            foreach (var ixx in ListImpl(data, key, parser, len))
            {
                if (ixx.Has)
                {
                    def = ixx.val;
                }
                yield return def;
            }
        }
        public static IEnumerable<T?> MergingList<T>(this EntityData data, string key, TryParser<T> parser, int len) where T : struct
        {
            return ListImpl(data, key, parser, len).Select(x => x.Has?x.val:default(T?));
        }
        public static IEnumerable<T?> MergingListC<T>(this EntityData data, string key, TryParser<T> parser, int len) where T : class
        {
            return ListImpl(data, key, parser, len).Select(x => x.Has?x.val:null);
        }
        public static Color HexaColor(this EntityData data, string key, Color? def = default)
        {
            var s = data.Attr(key, null);
            if (string.IsNullOrWhiteSpace(s) && def is Color dd)
            {
                return dd;
            }
            var ret = Calc.HexToColorWithAlpha(s);
            return ret;
        }
    }
}
