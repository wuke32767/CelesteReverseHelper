using Celeste.Mod.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.ReverseHelper.Libraries
{
    public struct TypeMatch
    {
        private HashSet<string> typesSet;
        private HashSet<Type> cachedTypesSet = [];

        public TypeMatch(string types)
        {
            typesSet = new HashSet<string>(types.Split(',').Select(x => x.Trim()));
        }

        public bool IsMatch(Type type)
        {
            if(cachedTypesSet.Contains(type))
            {
                return true;
            }
            var typesSet_cp = typesSet;
            bool flag=typesSet.Contains(type.FullName!)
                || typesSet.Contains(type.Name)
                || ((Attribute.GetCustomAttribute(type, typeof(CustomEntityAttribute)) as CustomEntityAttribute)?
                    .IDs?
                    .Select(x => x.Split('=')[0].Trim())
                    .Any(x => typesSet_cp.Contains(x!))
                    ?? false);
            if(flag)
            {
                cachedTypesSet.Add(type);
            }
            return flag;
                
        }
        public void Add(TypeMatch other)
        {
            typesSet.UnionWith(other.typesSet);
            cachedTypesSet.UnionWith(other.cachedTypesSet);
        }

        public bool Contains(Type type) => IsMatch(type);

        public static implicit operator TypeMatch(string v) => new TypeMatch(v);
    }
}