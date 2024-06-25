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
    internal static class Ext
    {
        public static Vector2 VHNormalize(this Vector2 self, Vector2 zero = default)
        {
            return self switch
            {
                { X: 0, Y: 0 } => zero,
                { X: 0 ,Y:var t} => new(0, Math.Sign(t)),
                { Y: 0,X:var t } => new(Math.Sign(t), 0),
                _ => throw new NotImplementedException("not a v/h line"),
            };
        }
        public static Vector2 Rotate90Clockwise(this Vector2 self, int times = 1)
        {
            return ((uint)times % 4) switch
            {
                0 => self,
                1 => new(-self.Y, self.X),
                2 => -self,
                3 => new(self.Y, -self.X),
                _ => throw new PlatformNotSupportedException("how?"),
            };
        }
        public static IEnumerable<(Vector2 from, Vector2 to)> CollideVHLine(this Collider collider, Vector2 from, Vector2 to)
        {//.SelectMany(x => x.CollideVHLine(from, to))
            if (collider is ColliderList cl)
            {
                return cl.colliders
                    .Aggregate((IEnumerable<(Vector2, Vector2)>)[(from, to)],
                    (arr, col) => arr.SelectMany(x => col.CollideVHLine(x.Item1, x.Item2)).ToArray());
            }

            var (qf, qq, qt) = (to - from) switch
            {
                { X: 0, Y: var t } v => (v, t),
                { Y: 0, X: var t } v => (v, t),
                _ => throw new NotImplementedException("not a v/h line"),
            }
            switch
            {
                { t: < 0 } u => (to, -u.v, u.t),
                { } u => (from, u.v, u.t),
            };
            return qq switch
            {
                { X: 0 } v => CollideVLine(collider, qf, qq),
                { Y: 0 } v => CollideHLine(collider, qf, qq),
                _ => [],
            }
            switch
            {
                { } a when qt < 0 => a.Select(x => (x.to, x.from)),
                { } a => a,
            };
        }
        public static IEnumerable<(Vector2 from, Vector2 to)> CollideHLine(this Collider collider, Vector2 from, Vector2 dst)
        {
            switch (collider)
            {
                case Hitbox hb:
                    {
                        var to = from + dst;
                        Rectangle rec = new((int)from.X, (int)from.Y, (int)dst.X, 1);
                        rec = Rectangle.Intersect(rec, hb.Bounds);
                        if (rec.Height > 0)
                        {
                            Vector2 fi = new(rec.Left, from.Y);
                            Vector2 sec = new(rec.Right, from.Y);
                            if (fi != from)
                            {
                                yield return (from, fi);
                            }
                            if (sec != to)
                            {
                                yield return (sec, to);
                            }
                        }
                        else
                        {
                            yield return (from, to);
                        }

                        break;

                    }
                case Grid gd:
                    {
                        var from2 = from;
                        var to = from + dst;
                        //swapo(ref from2.X, ref to.X);
                        var dst2 = to - from2;

                        var f2 = from2 - gd.AbsolutePosition;

                        var _f3 = (f2 / new Vector2(gd.CellWidth, gd.CellHeight)).Floor();
                        var _t3 = float.Ceiling((f2.X + dst2.X) / gd.CellWidth);

                        var fx = (int)_f3.X;
                        var y = (int)_f3.Y;
                        var t = (int)_t3;

                        bool beg = false;
                        int fr = 0;
                        for (int i = fx; i < t; i++)
                        {
                            if (!gd[i, y])
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
                                    yield return ClampLine(
                                        new Rectangle(
                                            (int)gd.AbsoluteLeft + (int)gd.CellWidth * fr,
                                            (int)gd.AbsoluteTop + (int)gd.CellHeight * y,
                                            (int)gd.CellWidth * (i - fr),
                                            (int)gd.CellHeight),
                                        from, from + dst);
                                }
                            }
                        }
                        if (beg)
                        {
                            beg = false;
                            yield return ClampLine(
                                new Rectangle(
                                    (int)gd.AbsoluteLeft + (int)gd.CellWidth * fr,
                                    (int)gd.AbsoluteTop + (int)gd.CellHeight * y,
                                    (int)gd.CellWidth * (t - fr),
                                    (int)gd.CellHeight),
                                from, from + dst);
                        }

                        break;
                    }
                default:
                    break;
            }

            ;
        }
        public static IEnumerable<(Vector2 from, Vector2 to)> CollideVLine(this Collider collider, Vector2 from, Vector2 dst)
        {
            switch (collider)
            {
                case Hitbox hb:
                    {
                        var to = from + dst;
                        Rectangle rec = new((int)from.X, (int)from.Y, 1, (int)dst.Y);
                        rec = Rectangle.Intersect(rec, hb.Bounds);
                        if (rec.Width > 0)
                        {
                            Vector2 fi = new(from.X, rec.Top);
                            Vector2 sec = new(from.X, rec.Bottom);
                            if (fi != from)
                            {
                                yield return (from, fi);
                            }
                            if (sec != to)
                            {
                                yield return (sec, to);
                            }
                        }
                        else
                        {
                            yield return (from, to);
                        }

                        break;
                    }
                case Grid gd:
                    {
                        var from2 = from;
                        var to = from + dst;
                        //swapo(ref from2.Y, ref to.Y);
                        var dst2 = to - from2;

                        var f2 = from2 - gd.AbsolutePosition;

                        var _f3 = (f2 / new Vector2(gd.CellWidth, gd.CellHeight)).Floor();
                        var _t3 = float.Ceiling((f2.Y + dst2.Y) / gd.CellWidth);

                        var fx = (int)_f3.X;
                        var y = (int)_f3.Y;
                        var t = (int)_t3;

                        bool beg = false;
                        int fr = 0;
                        for (int i = y; i < t; i++)
                        {
                            if (!gd[fx, i])
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
                                    yield return ClampLine(
                                      new Rectangle(
                                          (int)gd.AbsoluteLeft + (int)gd.CellWidth * fx,
                                          (int)gd.AbsoluteTop + (int)gd.CellHeight * fr,
                                          (int)gd.CellWidth,
                                          (int)gd.CellHeight * (i - fr)),
                                      from, from + dst);
                                }
                            }
                        }
                        if (beg)
                        {
                            beg = false;
                            yield return ClampLine(
                              new Rectangle(
                                  (int)gd.AbsoluteLeft + (int)gd.CellWidth * fx,
                                  (int)gd.AbsoluteTop + (int)gd.CellHeight * fr,
                                  (int)gd.CellWidth,
                                  (int)gd.CellHeight * (t - fr)),
                              from, from + dst);
                        }
                        break;
                    }
                default:
                    break;
            }

            ;
        }

        static (float from, float to)? TryClampLine(float xf, float xt, float yf, float yt)
        {
            //var mov = Math.Sign(xt - xf);
            //swapo(ref xf, ref xt);
            //swapo(ref yf, ref yt);
            float f = Math.Max(xf, yf), t = Math.Min(xt, yt);
            if (t < f)
            {
                return null;
            }

            //swapo(ref f, ref t, Math.Sign(mov));
            return (f, t);
        }

        static void swapo(ref float f, ref float t, int tar = 1)
        {
            if ((t > f) != (tar > 0))
            {
                (f, t) = (t, f);
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

        static Action<Entity>? _update;

        [MonoMod.MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Entity_Update(this Entity _)
        {
            //if not relinked
            _update ??= typeof(Entity).GetMethod(nameof(Entity.Update))!.CreateDelegate<Action<Entity>>();
            _update(_);
        }

        static Action<Entity>? _render;

        [MonoMod.MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Entity_Render(this Entity _)
        {
            //if not relinked
            _render ??= typeof(Entity).GetMethod(nameof(Entity.Render))!.CreateDelegate<Action<Entity>>();
            _render(_);
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