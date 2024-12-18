﻿global using Mono.Cecil.Cil;
global using Monocle;
global using MonoMod.Cil;
global using MonoMod.RuntimeDetour;
global using MonoMod.Utils;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using static Celeste.Mod.ReverseHelper.Libraries.ReflectionExt;
#pragma warning disable CS9113

namespace Celeste.Mod.ReverseHelper
{
    namespace SourceGen.Loader
    {
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        public class PreloadAttribute(params string[] id) : Attribute()
        {
        }
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public class LoadAttribute : Attribute
        {
        }
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        public class LazyLoadAttribute(string cond = null) : Attribute
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
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        public class DependencyAttribute(params Type[] type) : Attribute()
        {
        }
        //[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        //public class DependencyAttribute<T>() : Attribute()
        //{
        //}
    }
    namespace SourceGen
    {
        [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
        public class GeneratedAttribute : Attribute
        {
        }
    }

}


