using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.ReverseHelper.Library
{
    public class TypeMatch 
    {
        private HashSet<string> typesSet;

        public TypeMatch(string types)
        {
            typesSet = new HashSet<string>(types.Split(',').Select(x => x.Trim()));
        }

        public bool isMatch(Type type)
        {
            return typesSet.Contains(type.FullName) || typesSet.Contains(type.Name);
        }

        public bool Contains(Type type) => isMatch(type);

        public static implicit operator TypeMatch(string v) => new TypeMatch(v);
    }
}