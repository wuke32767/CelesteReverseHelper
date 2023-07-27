using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.ReverseHelper.Libraries
{
    public struct TypeMatch 
    {
        private HashSet<string> typesSet;

        public TypeMatch(string types)
        {
            typesSet = new HashSet<string>(types.Split(',').Select(x => x.Trim()));
        }

        public bool IsMatch(Type type)
        {
            return typesSet.Contains(type.FullName) || typesSet.Contains(type.Name);
        }
        public void Add(TypeMatch other)
        {

        }

        public bool Contains(Type type) => IsMatch(type);

        public static implicit operator TypeMatch(string v) => new TypeMatch(v);
    }
}