using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.ReverseHelper.Libraries
{
    internal static class Ext
    {
        internal static T? ExactlyOneOrDefault<T>(this IEnumerable<T> self)
        {
            using var e = self.GetEnumerator();
            if (e.MoveNext())
            {
                var r = e.Current;
                if (!e.MoveNext())
                {
                    return r;
                }
            }
            return default;
        }
    }
}
