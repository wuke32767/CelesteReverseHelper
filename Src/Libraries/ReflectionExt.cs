using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.ReverseHelper.Libraries
{
    internal static class ReflectionExt
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo methodof(Delegate @delegate)
        {
            return @delegate.Method;
        }
        public static Hook On(this MethodInfo info, Delegate @delegate)
        {
            return new Hook(info, @delegate);
        }
        public static ILHook ILState(this MethodInfo info, params (int at,Action<ILCursor> @delegate)[] at)
        {
            return new ILHook(info, il =>
            {
                ILCursor ic = new(il);
                ILLabel[] lb = null!;
                ic.GotoNext(i => i.MatchSwitch(out lb!));
                foreach (var (i,@delegate) in at)
                {
                    ILCursor get = new(il) { Next = lb[i].Target };
                    @delegate(get);
                }
            });
        }
        public static ILHook IL(this MethodInfo info, ILContext.Manipulator @delegate)
        {
            return new ILHook(info, @delegate);
        }
    }
}
