global using Mono.Cecil.Cil;
global using Monocle;
global using MonoMod.Cil;
global using MonoMod.RuntimeDetour;
global using MonoMod.Utils;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using static Celeste.Mod.ReverseHelper.Libraries.ReflectionExt;

namespace Celeste.Mod.ReverseHelper
{
    namespace SourceGen.Loader
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class LoadAttribute : Attribute
        {
        }
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class LazyLoadAttribute : Attribute
        {
        }
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
        public class LazyLoadDirectoryAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class UnloadAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class LoadContentAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class LoaderAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class UnloaderAttribute : Attribute
        {
        }
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class LoadContenterAttribute : Attribute
        {
        }
#pragma warning disable CS9113
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        public class DependencyAttribute(Type type) : Attribute()
        {
        }
#pragma warning restore CS9113
    }
    namespace SourceGen
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class GeneratedAttribute : Attribute
        {
        }
    }

}
