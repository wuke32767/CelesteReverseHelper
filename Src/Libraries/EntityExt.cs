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
            if (collider is ColliderList cl)
            {
                return cl.colliders.SelectMany(x => x.CollideVHLine(from, to));
            }
            return (to - from) switch
            {
                { X: 0 } v => CollideVLine(collider, from, v),
                { Y: 0 } v => CollideHLine(collider, from, v),
                _ => [],
            };
        }
        public static IEnumerable<(Vector2 from, Vector2 to)> CollideHLine(this Collider collider, Vector2 from, Vector2 dst)
        {
            switch (collider)
            {
                case Hitbox hb:
                    {
                        var to = from + dst;
                        if (from.Y < hb.AbsoluteBottom && hb.AbsoluteTop <= from.Y)
                        {
                            var r = TryClampLine(from.X, to.X, hb.AbsoluteLeft, hb.AbsoluteRight);
                            if (r is not null)
                            {
                                var (f, t) = r.Value;
                                if (f != from.X)
                                {
                                    yield return (new(from.X, from.Y), new(f, from.Y));
                                }
                                if (t != to.X)
                                {
                                    yield return (new(t, from.Y), new(to.X, from.Y));
                                }
                            }
                        }
                        break;
                    }
                case Grid gd:
                    {
                        var f2 = from - gd.AbsolutePosition;

                        var _f3 = (f2 / new Vector2(gd.CellWidth, gd.CellHeight)).Floor();
                        var _t3 = float.Ceiling((f2.X + dst.X) / gd.CellWidth);

                        var fx = (int)_f3.X;
                        var y = (int)_f3.Y;
                        var t = (int)_t3;
                        swap(ref fx, ref t);

                        bool beg = false;
                        int fr = 0;
                        for (int i = fx; i < t; i++)
                        {
                            if (!gd[fx, y])
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
                    break;
                case Grid gd:
                    break;
                default:
                    break;
            };
            yield break;
        }
        static (float from, float to)? TryClampLine(float xf, float xt, float yf, float yt)
        {
            var mov = Math.Sign(xt - xf);
            swap(ref xf, ref xt);
            swap(ref yf, ref yt);
            float f = Math.Max(xf, yf), t = Math.Min(xt, yt);
            if (t < f)
            {
                return null;
            }
            swap(ref f, ref t, Math.Sign(mov));
            return (f, t);
        }
        static void swap(ref float f, ref float t, int tar = 1)
        {
            if ((t > f) != (tar > 0))
            {
                (f, t) = (t + tar, f + tar);
            }
        }
        static void swap(ref int f, ref int t, int tar = 1)
        {
            if ((t > f) != (tar > 0))
            {
                (f, t) = (t + tar, f + tar);
            }
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
        public static Point ToPoint(this Vector2 vec)
        {
            return new Point((int)vec.X, (int)vec.Y);
        }
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
