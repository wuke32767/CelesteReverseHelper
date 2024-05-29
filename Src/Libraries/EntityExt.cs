using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.ReverseHelper.Libraries
{
    internal static class ColliderExt
    {
        public static IEnumerable<(Vector2 from, Vector2 to)> CollideVHLine(this Collider collider, Vector2 from, Vector2 to)
        {
            return (to - from) switch
            {
                Vector2 { X: 0 } v => CollideVLine(collider, from, v),
                Vector2 { Y: 0 } v => CollideHLine(collider, from, v),
                _ => [],
            };
        }
        public static IEnumerable<(Vector2 from, Vector2 to)> CollideHLine(this Collider collider, Vector2 from, Vector2 dst)
        {
            switch (collider)
            {
                case Hitbox hb:
                    {
                        var r = TryClampLine(collider, from, from + dst);
                        if (r is not null)
                        {
                            yield return r.Value;
                        }
                        break;
                    }
                case Grid gd:
                    {
                        var f2 = from - gd.AbsolutePosition;

                        var _f3 = (f2 / new Vector2(gd.CellWidth, gd.CellHeight)).Floor();
                        var _t3 = float.Ceiling((f2.X + dst.X - Math.Sign(dst.X)) / gd.CellWidth);

                        var fx = (int)_f3.X;
                        var y = (int)_f3.Y;
                        var t = (int)_t3;

                        bool beg = false;
                        int fr = 0;
                        for (int i = 0; i < t; i++)
                        {
                            if (gd.Data[fx + i, y])
                            {
                                if (!beg)
                                {
                                    fr = i;
                                    beg = true;
                                }
                            }
                            else
                            {
                                if (beg)
                                {
                                    beg = false;
                                    yield return ClampLine(new Rectangle((int)gd.AbsoluteLeft + (int)gd.CellWidth * fr, (int)gd.AbsoluteTop, (int)gd.CellWidth * (i - fr), (int)gd.CellHeight), from, from + dst);
                                }
                            }
                        }
                        //todo:optimization
                        break;
                    }
                default:
                    break;
            };
        }
        public static IEnumerable<(Vector2 from, Vector2 to)> CollideVLine(this Collider collider, Vector2 from, Vector2 dst)
        {
            switch (collider)
            {
                case Hitbox:
                    var r = TryClampLine(collider, from, from + dst);
                    if (r is not null)
                    {
                        yield return r.Value;
                    }
                    break;
                case Grid gd:

                    //todo
                    break;
                default:
                    break;
            };

        }
        static (Vector2 from, Vector2 to)? TryClampLine(Collider collider, Vector2 from, Vector2 to)
        {
            return TryClampLine(collider.Bounds, from, to);
        }
        static (Vector2 from, Vector2 to)? TryClampLine(Rectangle collider, Vector2 from, Vector2 to)
        {
            var d = to - from;
            Rectangle r = new((int)from.X, (int)from.Y, (int)d.X, (int)d.Y);

            //if (collider.Intersects())
            {
                return (from.Clamp(collider.Left, collider.Top, collider.Right - 1, collider.Bottom - 1),
                to.Clamp(collider.Left - 1, collider.Top - 1, collider.Right, collider.Bottom));
            }
            return (new(), new());
        }
        static (Vector2 from, Vector2 to) ClampLine(Rectangle collider, Vector2 from, Vector2 to)
        {
            return (from.Clamp(collider.Left, collider.Top, collider.Right - 1, collider.Bottom - 1),
                to.Clamp(collider.Left - 1, collider.Top - 1, collider.Right, collider.Bottom));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Vector2 Clamp(this Vector2 val, float minX, float minY, float maxX, float maxY)
        {
            return new Vector2(MathHelper.Clamp(val.X, minX, maxX), MathHelper.Clamp(val.Y, minY, maxY));
        }
    }
    internal static class EntityExt
    {
        static ILHook? some_il_hook;
        [MonoMod.MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Entity_Update(this Entity _)
        {
            //throw new NotImplementedException("How? This should be relinked.");
            var self = typeof(EntityExt).GetMethod(nameof(Entity_Update), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            some_il_hook = new ILHook(self!, il =>
            {
                il.IL.Clear();
                var ic = new ILCursor(il);
                ic.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_0);
                ic.Emit(Mono.Cecil.Cil.OpCodes.Callvirt, typeof(Entity).GetMethod(nameof(Entity.Update))!);
                ic.Emit(Mono.Cecil.Cil.OpCodes.Ret);
            });
        }
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
