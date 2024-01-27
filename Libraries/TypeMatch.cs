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

        public TypeMatch(string types)
        {
            typesSet = new HashSet<string>(types.Split(',').Select(x => x.Trim()));
        }

        public bool IsMatch(Type type)
        {
            var typesSet_cp = typesSet;
            return
                typesSet.Contains(type.FullName)
                || (typesSet.Contains(type.Name)
                || (type
                    .CustomAttributes
                    .FirstOrDefault(x => x.AttributeType == typeof(CustomEntityAttribute))?
                    .ConstructorArguments
                    .First()
                    .Value as System.Collections.ObjectModel.ReadOnlyCollection<CustomAttributeTypedArgument>)
                    .Select(x => (x.Value as string)!.Split('=')[0].Trim())
                    .Any(x => typesSet_cp.Contains(x!)));
        }
        public void Add(TypeMatch other)
        {

        }

        public bool Contains(Type type) => IsMatch(type);

        public static implicit operator TypeMatch(string v) => new TypeMatch(v);
    }
}