using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.ReverseHelper.Libraries
{
    internal static class EntityExt
    {
        public static IEnumerable<T> CollidableAll<T>(this Entity entity) where T : Entity
        {
            foreach (T s in entity.Scene.Tracker.GetEntities<T>())
            {
                if (s.Collider != null && entity.Collider.Collide(s))
                {
                    {
                        yield return s;
                        //var r = GetDreamifier(s);
                        //if (r is not null)
                        //{
                        //    var list = table.GetOrCreateValue(s);
                        //    list.AddRange(r.OfType<DreamBlock>());
                        //    Scene.Add(r);
                        //}
                    }
                }
            }
        }
    }
}
