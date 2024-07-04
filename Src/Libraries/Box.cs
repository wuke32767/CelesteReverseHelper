using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Celeste.Mod.ReverseHelper.Libraries
{
    class Box<T>() where T : struct
    {
        public T val = default;
        public static implicit operator Box<T>(T d) => new() { val = d };
        public static implicit operator T(Box<T> b) => b.val;
    }
}
