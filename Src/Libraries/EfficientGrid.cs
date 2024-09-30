using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.ReverseHelper.Libraries
{
    /// <summary>
    /// 0x0123456789abcdef represents:
    ///         top
    ///       00000001        (0x01)
    ///       00100011        (0x23)
    ///       01000101        (0x45)
    /// left  01100111  right (0x67)
    ///       10001001        (0x89)
    ///       10101011        (0xab)
    ///       11001101        (0xcd)
    ///       11101111        (0xef)
    ///        bottom
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct Unit88 : IEquatable<Unit88>
    {
        const ulong LeftTop = 0x8000_0000_0000_0000;
        public static readonly Unit88 template = new Unit88() { value = ulong.MaxValue };
        static readonly ulong[] mask;
        static Unit88()
        {
            ulong baseval = 1;
            List<ulong> arr = [0];
            for (int i = 0; i < 8; i++)
            {
                baseval = baseval | (baseval << 8);
            }
            //0x0101010101010101
            for (int i = 0; i < 8; i++)
            {
                arr.Add(arr[^1] | baseval);
                baseval <<= 1;
            }
            mask = arr.ToArray();
            //= [0x0000000000000000,0x0101010101010101,0x0303030303030303,0x0707070707070707,
            //0x0f0f0f0f0f0f0f0f,0x1f1f1f1f1f1f1f1f,0x3f3f3f3f3f3f3f3f,0x7f7f7f7f7f7f7f7f,0xffffffffffffffff]
        }
        public ulong value;
        public void Top(int shrink)
        {
            shrink *= 8;
            value <<= shrink;
            value >>= shrink;
        }
        public void Bottom(int shrink)
        {
            shrink *= 8;
            value >>= shrink;
            value <<= shrink;
        }
        public void Right(int shrink)
        {
            value &= ~mask[shrink];
        }
        public void Left(int shrink)
        {
            value &= mask[8 - shrink];
        }
        public static Unit88 operator |(Unit88 l, Unit88 r)
        {
            return new() { value = l.value | r.value };
        }
        public static Unit88 operator &(Unit88 l, Unit88 r)
        {
            return new() { value = l.value & r.value };
        }

        public static bool operator ==(Unit88 left, Unit88 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Unit88 left, Unit88 right)
        {
            return !(left == right);
        }

        bool InBound(int x, int y)
        {
            return x > 0 && y > 0 && x < 8 && y < 8;
        }

        public override bool Equals(object? obj)
        {
            return obj is Unit88 unit && Equals(unit);
        }

        public bool Equals(Unit88 other)
        {
            return value == other.value;
        }

        public bool this[int x, int y]
        {
            get
            {
                return (value & LeftTop >> (x + y * 8)) != 0;
            }
            set
            {
                if (value)
                {
                    this.value |= LeftTop >> (x + y * 8);
                }
                else
                {
                    this.value &= ~(LeftTop >> (x + y * 8));
                }
            }
        }
    }
    internal interface VirtualVirualMap
    {
        public int col { get; }
        public int row { get; }
        public Unit88 this[int x, int y] { get; set; }
        public void init(int col, int row);
    }
    internal struct VirtualMapWrapper() : VirtualVirualMap
    {
        public VirtualMap<Unit88> vm;

        public int col => vm.Columns;

        public int row => vm.Rows;

        public void init(int col, int row)
        {
            vm = new(col, row);
        }
        public Unit88 this[int x, int y]
        {
            get => vm[x, y];
            set => vm[x, y] = value;
        }
    }
    internal struct ArrayWrapper() : VirtualVirualMap
    {
        public Unit88[,] vm;

        public int col => vm.GetLength(0);

        public int row => vm.GetLength(1);

        public void init(int col, int row)
        {
            vm = new Unit88[col, row];
        }
        bool InBound(int x, int y)
        {
            return x >= 0 && y >= 0 && x < vm.GetLength(0) && y < vm.GetLength(1);
        }
        public Unit88 this[int x, int y]
        {
            get => InBound(x, y) ? vm[x, y] : default;
            set
            {
                if (InBound(x, y))
                {
                    vm[x, y] = value;
                }
            }
        }
    }

    internal interface IEfficientGrid
    {
        bool CheckRect(int x, int y, int xt, int yt);
        void Circle(int x, int y, int rad);
        void Debug_RenderAt(Vector2 at);
        void Rect(int x, int y, int xt, int yt);
    }

    internal struct EfficientGrid<T> : IEfficientGrid where T : VirtualVirualMap, new()
    {
        T data;
        public EfficientGrid(int col, int row)
        {
            data = new T();
            data.init((col + 7) / 8, (row + 7) / 8);
        }
        public void Debug_RenderAt(Vector2 at)
        {
            for (int i = 0; i < data.col; i++)
            {
                for (int j = 0; j < data.row; j++)
                {
                    var cur = data[i, j];
                    for (int k = 0; k < 8; k++)
                    {
                        for (int l = 0; l < 8; l++)
                        {
                            if (cur[k, l])
                            {
                                Draw.Point(at + new Vector2(i * 8 + k, j * 8 + l), Color.Azure);
                            }
                        }
                    }
                }
            }
        }
        public void Circle(int x, int y, int rad)
        {
            for (int i = 0; i < rad; i++)
            {
                int j = (int)double.BitDecrement(Math.Sqrt(rad * rad - i * i));
                Rect(x + i, y - j - 1, x + i + 1, y + j + 1);
                Rect(x - i - 1, y - j - 1, x - i, y + j + 1);
            }
            //Rect(x,)
        }
        public bool CheckRect(int x, int y, int xt, int yt)
        {
            x = Calc.Clamp(x, 0, data.col * 8);
            xt = Calc.Clamp(xt, 0, data.col * 8);
            y = Calc.Clamp(y, 0, data.row * 8);
            yt = Calc.Clamp(yt, 0, data.row * 8);
            bool flag = false;
            _Rect(x, y, xt, yt, (@in, mask) =>
            {
                if ((@in & mask) != default)
                {
                    flag = true;
                }
                return (flag, @in);
            });
            return flag;
        }
        public void Rect(int x, int y, int xt, int yt)
        {
            _Rect(x, y, xt, yt, (@in, mask) => (false, @in | mask));
        }
        void _Rect(int x, int y, int xt, int yt, Func<Unit88, Unit88, (bool, Unit88)> io)
        {
            int mapx = x / 8, mapy = y / 8,
                mapxt = (xt + 7) / 8, mapyt = (yt + 7) / 8;
            x &= 7;
            y &= 7;
            xt = 8 - xt;
            yt = 8 - yt;
            xt &= 7;
            yt &= 7;
            for (int i = mapx; i < mapxt; i++)
            {
                for (int j = mapy; j < mapyt; j++)
                {
                    var res = Unit88.template;
                    if (i == mapx)
                    {
                        res.Left(x);
                    }
                    if (j == mapy)
                    {
                        res.Top(y);
                    }
                    if (i == mapxt - 1)
                    {
                        res.Right(xt);
                    }
                    if (j == mapyt - 1)
                    {
                        res.Bottom(yt);
                    }
                    (var @out, data[i, j]) = io(data[i, j], res);
                    if (@out)
                    {
                        return;
                    }
                }
            }
        }
    }
    [Tracked]
    class EfficientPlayerCollider(Vector2 offset, Vector2 size, Action<Player> OnCollide) : Component(true, false)
    {
        EfficientGrid<ArrayWrapper> grid = new((int)size.X, (int)size.Y);
        public Vector2 offset = offset;
        public bool Check(Player p)
        {
            var off = offset + Entity.Position;
            int offx = (int)off.X;
            int offy = (int)off.Y;
            if (p.Collider is Hitbox { AbsoluteLeft: var l, AbsoluteRight: var r, AbsoluteTop: var t, AbsoluteBottom: var b }
                && grid.CheckRect((int)l - offx, (int)t - offy, (int)r - offx, (int)b - offy))
            {
                OnCollide(p);
                return true;
            }
            return false;
        }
        public void AddCollider(Collider[] value)
        {
            foreach (var col in value)
            {
                col.Position -= offset;
                switch (col)
                {
                    case Hitbox hb:
                        grid.Rect((int)hb.Left, (int)hb.Top, (int)hb.Right, (int)hb.Bottom);
                        break;
                    case Circle lb:
                        grid.Circle((int)lb.CenterX, (int)lb.CenterY, (int)lb.Radius);
                        break;
                    default:
                        break;
                }
            }
        }
        [SourceGen.Loader.Load]
        public static void Load()
        {
            On.Celeste.Player.Update += Player_Update;
        }
        [SourceGen.Loader.Unload]
        public static void Unload()
        {
            On.Celeste.Player.Update -= Player_Update;
        }

        private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);
            var collider = self.Collider;
            
            self.Collider = self.hurtbox;
            foreach (EfficientPlayerCollider c in self.Scene.Tracker.GetComponents<EfficientPlayerCollider>())
            {
                c.Check(self);
                if (self.Dead)
                {
                    break;
                }
            }
            
            self.Collider = collider;
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            grid.Debug_RenderAt(offset + Entity.Position);
        }
    }
}
