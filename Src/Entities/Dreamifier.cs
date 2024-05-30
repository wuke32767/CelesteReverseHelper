﻿using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.ReverseHelper.Entities
{
    public enum Groupmode
    {
        none = 0, collider = 1, entity = 2, all = 3,
    }
    [SourceGen.Loader.Dependency(typeof(DepthTracker))]
    [CustomEntity($"ReverseHelper/Dreamifier")]
    public class Dreamifier(Vector2 position, int width, int height, Color color1, Color color2,
        Color color3, Color color4, Groupmode connect, Groupmode blend, TypeMatch typeMatch, bool fg)
        : Entity(position)
    {
        public static ConditionalWeakTable<Solid, List<DreamifierRenderer_Base>> table = new();
        public Dreamifier(EntityData e, Vector2 offset)
        : this(e.Position + offset, e.Width, e.Height,
        e.HexaColor("lineColor"), e.HexaColor("fillColor"),
        e.HexaColor("lineColorDeactivated"), e.HexaColor("fillColorDeactivated"),
        e.Enum("ConnectMode", Groupmode.none), e.Enum("BlendInMode", Groupmode.collider),
        e.Attr("ignoreType"), e.Bool("fgTile", true))
        {
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            AfterAdded.Reg(scene, delegate ()
            {
                Collider = new Hitbox(width, height);

                foreach (Solid s in this.CollidableAll<Solid>()
                .Where(x => fg || x != SceneAs<Level>().SolidTiles)
                .Where(x => !typeMatch.IsMatch(x.GetType()))
                .Where(x => x is not DreamBlock))
                {
                    var r = GetDreamifier(s);
                    if (r is not null)
                    {
                        var list = table.GetOrCreateValue(s);
                        list.AddRange(r.OfType<DreamifierRenderer_Base>());
                        Scene.Add(r);
                    }
                }
                //RemoveSelf();
                DreamifierRenderer_Base?[]? GetDreamifier(Solid solid) => GetDreamifier2(solid.Collider, solid, solid.Collider);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                DreamifierRenderer_Base?[]? GetDreamifier2(Collider collider, Solid solid, Collider groupby)
                {
                    switch (collider)
                    {
                        case Grid grid:
                            return [new DreamifierRenderer_Grid(Position, width, height, color1, color2, color3, color4, solid, grid, blend, connect)];
                        case Hitbox hitbox:
                            return [new DreamifierRenderer_Hitbox(Position, width, height, color1, color2, color3, color4, solid, hitbox, blend, connect)];
                        case ColliderList cl:
                        //return cl.colliders.SelectMany(x => GetDreamifier2(x, solid, cl) ?? []).ToArray();
                        //not supported
                        case Circle:
                        default:
                            break;
                    }
                    return null;
                }
            });
        }
        [SourceGen.Loader.Load]
        [SourceGen.Loader.LazyLoad]
        public static void Load()
        {
            On.Monocle.Scene.SetActualDepth += Scene_SetActualDepth;
        }

        private static void Scene_SetActualDepth(On.Monocle.Scene.orig_SetActualDepth orig, Scene self, Entity entity)
        {
            if (entity is not DreamifierRenderer_Base)
            {
                orig(self, entity);
            }
        }

        [SourceGen.Loader.Unload]
        public static void Unload()
        {
            On.Monocle.Scene.SetActualDepth -= Scene_SetActualDepth;
        }
    }
    [Tracked(true)]
    [TrackedAs(typeof(DreamBlock), true)]
    public abstract class DreamifierRenderer_Base : DreamBlock
    {
        protected List<(Vector2 from, Vector2 to)> WobbleList = [];
        protected List<Vector2> CornerList = [];
        public List<Rectangle> AltCollider = new();

        public Color lineColor;
        public Color fillColor;
        public Color linecolorDeact;
        public Color fillColorDeact;
        public Solid solid;
        public Collider _solidcollider;
        public Groupmode solidConnect;
        public Groupmode dreamConnect;


        internal DepthTracker depthTracker;

        StaticMover staticmover = default!;
        public override void Added(Scene scene)
        {
            WobbleList = prepareRenderer().Select(x => (x.Key, x.Value)).ToList();
            base.Added(scene);
            if (Collider.Bounds.IsEmpty)
            {
                Dreamifier.table.GetOrCreateValue(solid).Remove(this);
                RemoveSelf();
            }
        }
        protected abstract Dictionary<Vector2, Vector2> prepareRenderer();
        public override void Awake(Scene scene)
        {
            AllowStaticMovers = false;
            base.Awake(scene);
            depthTracker = DepthTracker.Track(solid, this);
            Setup();
            //offset = solid.Position - Position;

            Add(staticmover = new StaticMover());
            solid.staticMovers.Add(staticmover);
            staticmover.Platform = solid;
            staticmover.OnAttach?.Invoke(solid);

            var dashcollide = solid.OnDashCollide;
            if (dashcollide is not null)
            {
                solid.OnDashCollide = (p, dir) =>
                {
                    if (p.CollideCheck(this, p.Position + dir))
                    {
                        return DashCollisionResults.NormalOverride;
                    }
                    return dashcollide(p, dir);
                };
            }
            var bound = Collider.Bounds;
            bound.X -= 1; bound.Y -= 1;
            bound.Width += 2;
            bound.Height += 2;
            var con = dreamConnect switch
            {
                Groupmode.collider => Dreamifier.table.GetOrCreateValue(solid).Where(x => x._solidcollider == _solidcollider).ToList(),
                Groupmode.entity => Dreamifier.table.GetOrCreateValue(solid),
                Groupmode.all => Scene.CollideAll<DreamifierRenderer_Base>(bound),
                _ => [],
            };
            var blend = (solidConnect switch
            {
                Groupmode.collider => Enumerable.Repeat(_solidcollider, 1),
                Groupmode.entity => expand(solid.Collider),
                Groupmode.all => Scene.CollideAll<Solid>(bound).SelectMany(x => expand(x.Collider)),
                _ => Enumerable.Empty<Collider>(),
            }).Concat(con.Select(x => x.Collider)).Distinct().ToList();
            IEnumerable<Collider> expand(Collider c)
            {
                return c switch
                {
                    Grid grid => Enumerable.Repeat(c, 1),
                    Hitbox hitbox => Enumerable.Repeat(c, 1),
                    ColliderList cl => cl.colliders.SelectMany(x => expand(x)),
                    _ => Enumerable.Empty<Collider>(),
                };
            }

        }
        //public Vector2 offset;

        public new TilesetDreamBlock.DreamParticle[] particles;
        public new MTexture[] particleTextures =
        [
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(14, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(0, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7)
        ];

        private new float wobbleFrom = Calc.Random.NextFloat((float)Math.PI * 2f);

        private new float wobbleTo = Calc.Random.NextFloat((float)Math.PI * 2f);

        private new float wobbleEase;
        public new float animTimer;

        public DreamifierRenderer_Base(Vector2 position, int width, int height, Color line, Color block,
                Color linede, Color fillde, Solid solid, Collider collider, Groupmode solidconnect, Groupmode dreamconnect) : base(position, width, height, null, false, false, false)
        {
            lineColor = line;
            fillColor = block;
            linecolorDeact = linede;
            fillColorDeact = fillde;
            this.solid = solid;
            _solidcollider = collider;

            DreamBlockConfig.Get(this).HighPriority();
            //vanilla uses 10
            //for footstep ripple priority
            SurfaceSoundPriority = 11;
            depthTracker = null!;
            particles = null!;
            this.solid = solid;
            solidConnect = solidconnect;
            dreamConnect = dreamconnect;
        }

        public new void Setup()
        {
            particles = new TilesetDreamBlock.DreamParticle[(int)(Width / 8f * (Height / 8f) * 0.7f)];
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Position = new Vector2(Calc.Random.NextFloat(Width), Calc.Random.NextFloat(Height));
                particles[i].Layer = Calc.Random.Choose(0, 1, 1, 2, 2, 2);
                particles[i].TimeOffset = Calc.Random.NextFloat();
                particles[i].Colord = Color.LightGray * (0.5f + particles[i].Layer / 2f * 0.5f);
                //if (playerHasDreamDash)
                {
                    switch (particles[i].Layer)
                    {
                        case 0:
                            particles[i].Colora = Calc.Random.Choose(Calc.HexToColor("FFEF11"), Calc.HexToColor("FF00D0"), Calc.HexToColor("08a310"));
                            break;
                        case 1:
                            particles[i].Colora = Calc.Random.Choose(Calc.HexToColor("5fcde4"), Calc.HexToColor("7fb25e"), Calc.HexToColor("E0564C"));
                            break;
                        case 2:
                            particles[i].Colora = Calc.Random.Choose(Calc.HexToColor("5b6ee1"), Calc.HexToColor("CC3B3B"), Calc.HexToColor("7daa64"));
                            break;
                    }
                }
            }
        }
        public new bool playerHasDreamDash
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => DreamBlockConfigurer.dreamblock_enabled(this);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            depthTracker.RemoveSelf();
        }
        public override void Update()
        {
            if (solid.Scene is null)
            {
                RemoveSelf();
                return;
            }
            Collidable = solid.Collidable;
            Visible = solid.Visible;
            this.Entity_Update();
            if (playerHasDreamDash)
            {
                animTimer += 6f * Engine.DeltaTime;
                wobbleEase += Engine.DeltaTime * 2f;
                if (wobbleEase > 1f)
                {
                    wobbleEase = 0f;
                    wobbleFrom = wobbleTo;
                    wobbleTo = Calc.Random.NextFloat((float)Math.PI * 2f);
                }

                //SurfaceSoundIndex = 12;
            }
            else
            {
                //SurfaceSoundIndex = 11;
            }
            //MoveTo(solid.Position - offset);
        }
        public new void WobbleLine(Vector2 from, Vector2 to, float offset)
        {
            float num = (to - from).Length();
            Vector2 vector = Vector2.Normalize(to - from);
            Vector2 vector2 = new Vector2(vector.Y, 0f - vector.X) / 2f;

            float num2 = 0f;
            int num3 = 16;
            for (int i = 0; i < num; i += num3)
            {
                float num4 = Lerp(LineAmplitude(wobbleFrom + offset, i), LineAmplitude(wobbleTo + offset, i), wobbleEase);
                if (i + num3 >= num)
                {
                    num4 = 0f;
                }

                float num5 = Math.Min(num3, num - i);
                Vector2 vector3 = from + vector * i + vector2 * num2;
                Vector2 vector4 = from + vector * (i + num5) + vector2 * num4;
                Draw.Line(vector3, vector4, playerHasDreamDash ? lineColor : linecolorDeact);
                num2 = num4;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool Inside(Vector2 pos)
        {
            if (pos.X >= Left + 2f && pos.Y >= Top + 2f && pos.X < Right - 2f && pos.Y < Bottom - 2f)
            {
                return true;
            }
            return false;
        }

        public override void Render()
        {
            Camera camera = SceneAs<Level>().Camera;
            Rectangle camerarect = new Rectangle((int)camera.X, (int)camera.Y, 320, 180);

            if (!camerarect.Intersects(new((int)X, (int)Y, (int)Width, (int)Height)))
            {
                return;
            }

            Vector2 shake = new(0, 0);
            foreach (var r in AltCollider)
            {
                Draw.Rect(shake.X + r.X, shake.Y + r.Y, r.Width, r.Height, playerHasDreamDash ? fillColor : fillColorDeact);
            }
            Vector2 position = SceneAs<Level>().Camera.Position;
            for (int i = 0; i < particles.Length; i++)
            {
                int layer = particles[i].Layer;
                Vector2 position2 = particles[i].Position;
                position2 += position * (0.3f + 0.25f * layer);
                position2 = PutInside(position2);
                if (!Inside(position2))
                {
                    continue;
                }
                Color color = particles[i].Color(this);
                MTexture mTexture;
                switch (layer)
                {
                    case 0:
                        {
                            int num2 = (int)((particles[i].TimeOffset * 4f + animTimer) % 4f);
                            mTexture = particleTextures[3 - num2];
                            break;
                        }
                    case 1:
                        {
                            int num = (int)((particles[i].TimeOffset * 2f + animTimer) % 2f);
                            mTexture = particleTextures[1 + num];
                            break;
                        }
                    default:
                        mTexture = particleTextures[2];
                        break;
                }
                //if (position2.X >= 2f && position2.Y >= 2f && position2.X < Width - 2f && position2.Y < Height - 2f)
                {
                    mTexture.DrawCentered(position2 + shake, color);
                }
            }
            float off = 0;
            foreach (var (f, t) in WobbleList)
            {
                WobbleLine(f + Position, t + Position, off += 0.75f);
            }
            foreach (var r in CornerList)
            {
                Draw.Rect(shake + r + Position, 1, 1, playerHasDreamDash ? lineColor : linecolorDeact);
            }
        }
    }
    public class DreamifierRenderer_Grid(Vector2 position, int width, int height, Color line, Color block,
        Color linede, Color fillde, Solid solid, Grid collider, Groupmode solidconnect, Groupmode dreamconnect)
        : DreamifierRenderer_Base(position, width, height, line, block, linede, fillde, solid, collider, solidconnect, dreamconnect)
    {
        Grid solidtarget => (Grid)_solidcollider;

        protected override Dictionary<Vector2, Vector2> prepareRenderer()
        {
            Dictionary<Vector2, Vector2> WobbleList = [];
            //todo: these "8" are hardcoded
            //var scene = SceneAs<Level>();
            Solid solidTiles = solid;//scene.SolidTiles;
            Grid? solidCollider = solidtarget;
            var tilechar = solidCollider!.Data;// scene.SolidsData;

            Rectangle tileBounds = solidCollider.Bounds;//scene.Session.MapData.TileBounds;
            var tiletex = solidCollider.Data;//solidTiles.Tiles.Tiles;
            int x = (int)(X / 8f) - tileBounds.Left / 8;
            int y = (int)(Y / 8f) - tileBounds.Top / 8;
            int tilesX = (int)Width / 8;
            int tilesY = (int)Height / 8;
            var collidemap = new VirtualMap<bool>(tilesX, tilesY, false);
            //var tg = new TileGrid(8, 8, tilesX, tilesY);
            for (int i = 0; i < tilesX; i++)
            {
                for (int j = 0; j < tilesY; j++)
                {
                    if (tiletex[x + i, y + j] != tiletex.EmptyValue)
                    {
                        collidemap[i, j] = true;
                    }
                }
            }
            Collider = new Grid(8, 8, collidemap);
            //tileGrid=tg;
            //Add(tg);
            //tg.Visible = false;

            if (solidConnect < Groupmode.collider)
            {
                tilechar = collidemap;
            }
            for (int i = 0; i < tilesX; i++)
            {
                Vector2? froml = null, fromr = null;
                for (int j = 0; j < tilesY; j++)
                {
                    // build vertical edges of dream block.
                    if (tilechar[x + i, y + j] != tilechar.EmptyValue && tilechar[x + i - 1, y + j] == tilechar.EmptyValue)
                    {
                        if (froml is null)
                        {
                            froml = new(i * 8, j * 8);
                        }
                    }
                    else
                    {
                        if (froml is not null)
                        {
                            WobbleList.Add(new(i * 8, j * 8), froml.Value);
                            froml = null;
                        }
                    }
                    if (tilechar[x + i, y + j] != tilechar.EmptyValue && tilechar[x + i + 1, y + j] == tilechar.EmptyValue)
                    {
                        if (fromr is null)
                        {
                            fromr = new(i * 8, j * 8);
                        }
                    }
                    else
                    {
                        if (fromr is not null)
                        {
                            WobbleList.Add(fromr.Value + new Vector2(8, 0), new(8 + i * 8, j * 8));
                            fromr = null;
                        }
                    }
                }
                if (froml is not null)
                {
                    WobbleList.Add(new(i * 8, tilesY * 8), froml.Value);
                }
                if (fromr is not null)
                {
                    WobbleList.Add(fromr.Value + new Vector2(8, 0), new(8 + i * 8, tilesY * 8));
                }

            }
            for (int j = 0; j < tilesY; j++)
            {
                Vector2? fromu = null, fromd = null;
                for (int i = 0; i < tilesX; i++)
                {
                    // build horizonal edges of dream block.
                    if (tilechar[x + i, y + j] != tilechar.EmptyValue && tilechar[x + i, y + j - 1] == tilechar.EmptyValue)
                    {
                        if (fromu is null)
                        {
                            fromu = new(i * 8, j * 8);
                        }
                    }
                    else
                    {
                        if (fromu is not null)
                        {
                            WobbleList.Add(fromu.Value, new(i * 8, j * 8));
                            fromu = null;
                        }
                    }
                    if (tilechar[x + i, y + j] != tilechar.EmptyValue && tilechar[x + i, y + j + 1] == tilechar.EmptyValue)
                    {
                        if (fromd is null)
                        {
                            fromd = new(i * 8, j * 8);
                        }
                    }
                    else
                    {
                        if (fromd is not null)
                        {
                            WobbleList.Add(new(i * 8, j * 8 + 8), fromd.Value + new Vector2(0, 8));
                            fromd = null;
                        }
                    }
                }
                if (fromu is not null)
                {
                    WobbleList.Add(fromu.Value, new(tilesX * 8, j * 8));
                }
                if (fromd is not null)
                {
                    WobbleList.Add(new(tilesX * 8, j * 8 + 8), fromd.Value + new Vector2(0, 8));
                }
            }

            var other = collidemap.Clone();
            for (int i = 0; i < tilesX; i++)
            {
                for (int j = 0; j < tilesY; j++)
                {
                    if (other[i, j])
                    {
                        int it = i + 1;
                        int jt = j + 1;
                        other[i, j] = false;
                        while (jt != tilesY && other[i, jt])
                        {
                            other[i, jt] = false;
                            jt++;
                        }
                        while (it != tilesX)
                        {
                            if (!Enumerable.Range(j, jt - j).Select(j_ => other[it, j_]).Contains(false))
                            {
                                for (int j_ = j; j_ < jt; j_++)
                                {
                                    other[it, j_] = false;
                                }
                                it++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        AltCollider.Add(new((int)X + 8 * i, (int)Y + 8 * j, (it - i) * 8, (jt - j) * 8));
                    }
                }
            }
            return WobbleList;
        }

        // check if pos is inside Grid with 2 paddings.
        public override bool Inside(Vector2 pos)
        {
            // a 8*8 cell can be sliced into 9 pieces.
            // same number means they check same cell to determine if they is inside the grid.
            // 11222233
            // 11222233
            // 44555566
            // 44555566
            // 44555566
            // 44555566
            // 77888899
            // 77888899
            // notice that [1] is same as (left-up cell)[9], (left cell)[7], (up cell)[3],
            //             [2] is same as (up cell)[8],
            //             [4] is same as (left cell)[6],
            // (they check same cell)
            // so we move pos by (2, 2),
            pos.X += 2;
            pos.Y += 2;
            // and a cell should become 
            // 99778888
            // 99778888
            // 33112222
            // 33112222
            // 66445555
            // 66445555
            // 66445555
            // 66445555
            // just split it to 4 pieces.
            Rectangle tileBounds = solidtarget.Bounds;
            var tiledata = solidtarget.Data;
            // by just divide result by 4, (8 is a cell)
            int xm2 = (int)(pos.X / 8f * 2) - 2 * tileBounds.Left / 8;
            int ym2 = (int)(pos.Y / 8f * 2) - 2 * tileBounds.Top / 8;

            bool b = true;
            // and check lsb to determine which sub-cell it is in.
            for (int i = xm2 & 1; i < 2; i++)
            {
                for (int j = ym2 & 1; j < 2; j++)
                {
                    if (tiledata[xm2 / 2 - 1 + i, ym2 / 2 - 1 + j] == tiledata.EmptyValue)
                    {
                        b = false;
                        break;
                    }
                }
            }
            return b;
        }
        [SourceGen.Loader.Load]
        internal static void Load()
        {
            On.Celeste.DreamBlock.FootstepRipple += DreamBlock_FootstepRipple;
        }

        private static void DreamBlock_FootstepRipple(On.Celeste.DreamBlock.orig_FootstepRipple orig, DreamBlock self, Vector2 position)
        {
            if (self is DreamifierRenderer_Grid ex)
            {
                ex.FootstepRipple(position);
            }
            else
            {
                orig(self, position);
            }
        }
        public new void FootstepRipple(Vector2 position)
        {
            if (playerHasDreamDash)
            {
                //foreach (DreamifierRenderer_Grid rects in Scene.Tracker.Entities[typeof(DreamifierRenderer_Grid)])
                {
                    foreach (var rec in AltCollider)
                    {
                        var burst = SceneAs<Level>().Displacement.AddBurst(position, 0.5f, 0f, 40f);
                        burst.WorldClipRect = rec;
                        burst.WorldClipPadding = 1;
                    }
                }
            }
        }

        [SourceGen.Loader.Unload]
        internal static void Unload()
        {
            On.Celeste.DreamBlock.FootstepRipple -= DreamBlock_FootstepRipple;
        }
    }
    public class DreamifierRenderer_Hitbox(Vector2 position, int width, int height, Color line, Color block,
        Color linede, Color fillde, Solid solid, Hitbox collider, Groupmode solidconnect, Groupmode dreamconnect)
        : DreamifierRenderer_Base(position, width, height, line, block, linede, fillde, solid, collider, solidconnect, dreamconnect)
    {
        Hitbox solidtarget => (Hitbox)_solidcollider;

        protected override Dictionary<Vector2, Vector2> prepareRenderer()
        {
            var srect = solidtarget.Bounds;
            var rect = Rectangle.Intersect(Collider.Bounds, srect);
            Position = new(rect.X, rect.Y);
            Collider = new Hitbox(rect.Width, rect.Height);

            if (solidConnect < Groupmode.collider)
            {
                srect = rect;
            }
            Dictionary<Vector2,  Vector2 > WobbleList = [];

            if (srect.Top == rect.Top)
            {
                WobbleList.Add(Collider.TopLeft, Collider.TopRight);
            }
            if (srect.Bottom == rect.Bottom)
            {
                WobbleList.Add(Collider.BottomRight, Collider.BottomLeft);
            }
            if (srect.Left == rect.Left)
            {
                WobbleList.Add(Collider.BottomLeft, Collider.TopLeft);
            }
            if (srect.Right == rect.Right)
            {
                WobbleList.Add(Collider.TopRight, Collider.BottomRight);
            }

            AltCollider.Add(rect);
            return WobbleList;
        }
    }
}