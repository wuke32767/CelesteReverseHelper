using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.ReverseHelper.Libraries
{
    static class HookGen
    {
        static List<ILHook> Hooks = new List<ILHook>();
        public static void Gen(MethodBase On, Action<ILCursor> Hook)
        {
        }
    }
}
