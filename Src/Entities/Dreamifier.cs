using Celeste.Mod.Entities;
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
    internal class DepthTracker : Component
    {
        public Entity target;
        //if target.actualDepth is not set.
        //if it's not dirty, no need to update.
        bool dirty { get => Active; set => Active = value; }
        public DepthTracker(Entity trackto, Entity self) : base(true, false)
        {
            target = self;
            trackto.Add(this);
            Apply();
        }
        class some_cmp : IComparer<Entity>
        {
            public int Compare(Entity? a, Entity? b) => Math.Sign(b!.actualDepth - a!.actualDepth);
            public static some_cmp Instance = new();
        }
        public void Apply()
        {
            if (!Scene.Entities.unsorted)
            {
                var i = Scene.Entities.entities.BinarySearch(Entity, some_cmp.Instance);
                if (i >= 0 && Scene.Entities.entities[i] == Entity)
                {
                    int d = Entity.Depth;
                    Scene.actualDepthLookup[d] += 9.9999999747524271E-07;
                    foreach (var e in Scene.Entities.entities.Skip(i + 1).TakeWhile(e => e.Depth == d))
                    {
                        e.actualDepth -= 9.9999999747524271E-07;
                    }
                    target.actualDepth = Entity.actualDepth - 9.9999999747524271E-07;
                    Scene.Entities.MarkUnsorted();
                    dirty = false;
                    return;
                }
            }
        }
        public override void Update()
        {
            base.Update();
            //if (dirty)
            {
                Apply();
            }
        }
        [SourceGen.Loader.Load]
        public static void Load()
        {
            On.Monocle.Scene.SetActualDepth += Scene_SetActualDepth;
        }

        private static void Scene_SetActualDepth(On.Monocle.Scene.orig_SetActualDepth orig, Scene self, Entity entity)
        {
            orig(self, entity);
            foreach (var dt in entity.Components.OfType<DepthTracker>())
            {
                dt.target.Depth = entity.Depth;
                //if (dt.target.Scene is null)
                //{
                //    dt.RemoveSelf();
                //}
                dt.dirty = false;
            }
        }

        [SourceGen.Loader.Unload]
        public static void Unload()
        {
            On.Monocle.Scene.SetActualDepth -= Scene_SetActualDepth;
        }
    }


    [CustomEntity($"ReverseHelper/Dreamifier")]
    public class Dreamifier(Vector2 position, int width, int height, Color color1, Color color2, Color color3, Color color4) : Entity(position)
    {
        static ConditionalWeakTable<Solid, List<DreamBlock>> table = new();
        public Dreamifier(EntityData e, Vector2 offset)
        : this(e.Position + offset, e.Width, e.Height,
        e.HexaColor("lineColor"), e.HexaColor("fillColor"),
        e.HexaColor("lineColorDeactivated"), e.HexaColor("fillColorDeactivated"))
        {
        }
        public override void Awake(Scene scene)
        {
            Collider = new Hitbox(width, height);
            base.Awake(scene);

            foreach (Solid s in Scene.Tracker.Entities[typeof(Solid)])
            {
                if (s.Collider != null && Collider.Collide(s))
                {
                    {
                        var r = GetDreamifier(s);
                        if (r is not null)
                        {
                            var list = table.GetOrCreateValue(s);
                            list.Add(r);
                            Scene.Add(r);
                        }
                    }
                }
            }
            //RemoveSelf();
            DreamBlock? GetDreamifier(Solid solid)
            {
                switch (solid.Collider)
                {
                    case Grid:
                        return new DreamifierRenderer_Grid(position, width, height, color1, color2, color3, color4, solid);
                    case Hitbox:
                        return new DreamifierRenderer_Hitbox(position, width, height, color1, color2, color3, color4, solid);
                    //not supported
                    case Circle:
                        break;
                    case ColliderList:
                        break;
                }
                return null;
            }
        }
    }
    [Tracked]
    [TrackedAs(typeof(DreamBlock))]
    public class DreamifierRenderer_Grid : DreamBlock
    {
        //public static DreamifierRenderer_Grid ConstructDreamifier(Level level, LevelData levelData, Vector2 offset, EntityData data)
        //{
        //    return new DreamifierRenderer_Grid(data, offset);
        //}

        List<(Vector2 from, Vector2 to)> WobbleList = [];
        List<Vector2> CornerList = [];

        Solid solid;

        public Color lineColor;
        public Color fillColor;
        public Color linecolorDeact;
        public Color fillColorDeact;

        public DreamifierRenderer_Grid(Vector2 position, int width, int height, Color line, Color block,
            Color linede, Color fillde, Solid solid) : base(position, width, height, null, false, false, false)
        {
            lineColor = line;
            fillColor = block;
            linecolorDeact = linede;
            fillColorDeact = fillde;
            DreamBlockConfig.Get(this).HighPriority();
            //vanilla uses 10
            //for footstep ripple priority
            SurfaceSoundPriority = 11;
            Renderer = null!;
            particles = null!;
            this.solid = solid;
        }


        DepthTracker Renderer;
        public override void Awake(Scene _scene)
        {
            //todo: these "8" are hardcoded
            base.Awake(_scene);
            //var scene = SceneAs<Level>();
            Solid solidTiles = solid;//scene.SolidTiles;
            Grid? solidCollider = solidTiles.Collider as Grid;
            var tilechar = solidCollider!.Data;// scene.SolidsData;

            solidTiles.Add(Renderer = new DepthTracker(solidTiles, this));
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
                        //tg.Tiles[i, j] = tiletex[x + i, y + j];
                    }
                }
            }
            Collider = new Grid(8, 8, collidemap);
            //tileGrid=tg;
            //Add(tg);
            //tg.Visible = false;

            for (int i = 0; i < tilesX; i++)
            {
                for (int j = 0; j < tilesY; j++)
                {
                    if (tilechar[x + i, y + j] != tilechar.EmptyValue)
                    {
                        if (tilechar[x + i + 1, y + j] == tilechar.EmptyValue && tilechar[x + i, y + j + 1] == tilechar.EmptyValue)
                        {
                            CornerList.Add(new(i * 8 + 6, j * 8 + 6));
                        }
                        if (tilechar[x + i, y + j + 1] == tilechar.EmptyValue && tilechar[x + i - 1, y + j] == tilechar.EmptyValue)
                        {
                            CornerList.Add(new(i * 8 + 1, j * 8 + 6));
                        }
                        if (tilechar[x + i - 1, y + j] == tilechar.EmptyValue && tilechar[x + i, y + j - 1] == tilechar.EmptyValue)
                        {
                            CornerList.Add(new(i * 8 + 1, j * 8 + 1));
                        }
                        if (tilechar[x + i, y + j - 1] == tilechar.EmptyValue && tilechar[x + i + 1, y + j] == tilechar.EmptyValue)
                        {
                            CornerList.Add(new(i * 8 + 6, j * 8 + 1));
                        }
                        if (tilechar[x + i + 1, y + j] != tilechar.EmptyValue
                         && tilechar[x + i, y + j + 1] != tilechar.EmptyValue
                         && tilechar[x + i + 1, y + j + 1] == tilechar.EmptyValue)
                        {
                            CornerList.Add(new(i * 8 + 7, j * 8 + 7));
                        }
                        if (tilechar[x + i, y + j + 1] != tilechar.EmptyValue
                         && tilechar[x + i - 1, y + j] != tilechar.EmptyValue
                         && tilechar[x + i - 1, y + j + 1] == tilechar.EmptyValue)
                        {
                            CornerList.Add(new(i * 8 + 0, j * 8 + 7));
                        }
                        if (tilechar[x + i - 1, y + j] != tilechar.EmptyValue
                         && tilechar[x + i, y + j - 1] != tilechar.EmptyValue
                         && tilechar[x + i - 1, y + j - 1] == tilechar.EmptyValue)
                        {
                            CornerList.Add(new(i * 8 + 0, j * 8 + 0));
                        }
                        if (tilechar[x + i, y + j - 1] != tilechar.EmptyValue
                         && tilechar[x + i + 1, y + j] != tilechar.EmptyValue
                         && tilechar[x + i + 1, y + j - 1] == tilechar.EmptyValue)
                        {
                            CornerList.Add(new(i * 8 + 7, j * 8 + 0));
                        }
                    }
                }
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
                            WobbleList.Add((new(i * 8, j * 8), froml.Value));
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
                            WobbleList.Add((fromr.Value + new Vector2(8, 0), new(8 + i * 8, j * 8)));
                            fromr = null;
                        }
                    }
                }
                if (froml is not null)
                {
                    WobbleList.Add((new(i * 8, tilesY * 8), froml.Value));
                }
                if (fromr is not null)
                {
                    WobbleList.Add((fromr.Value + new Vector2(8, 0), new(8 + i * 8, tilesY * 8)));
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
                            WobbleList.Add((fromu.Value, new(i * 8, j * 8)));
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
                            WobbleList.Add((new(i * 8, j * 8 + 8), fromd.Value + new Vector2(0, 8)));
                            fromd = null;
                        }
                    }
                }
                if (fromu is not null)
                {
                    WobbleList.Add((fromu.Value, new(tilesX * 8, j * 8)));
                }
                if (fromd is not null)
                {
                    WobbleList.Add((new(tilesX * 8, j * 8 + 8), fromd.Value + new Vector2(0, 8)));
                }
            }

            Setup();
            //Add(new BeforeRenderHook(BeforeRender));

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
            offset = solid.Position - Position;
        }
        Vector2 offset;

        List<Rectangle> AltCollider = new();
        enum _edges : byte
        {
            up = 1, left = 2, right = 4, down = 8,
        }

        new TilesetDreamBlock.DreamParticle[] particles;
        public new void Setup()
        {


            particles = new TilesetDreamBlock.DreamParticle[(int)(base.Width / 8f * (base.Height / 8f) * 0.7f)];
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Position = new Vector2(Calc.Random.NextFloat(base.Width), Calc.Random.NextFloat(base.Height));
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
            Renderer.RemoveSelf();
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
        private new void WobbleLine(Vector2 from, Vector2 to, float offset)
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
        private static new float Lerp(float a, float b, float percent)
        {
            return a + (b - a) * percent;
        }
        private static new float LineAmplitude(float seed, float index)
        {
            return (float)(Math.Sin((double)(seed + index / 16f) + Math.Sin(seed * 2f + index / 32f) * 6.2831854820251465) + 1.0) * 1.5f;
        }

        private new float wobbleFrom = Calc.Random.NextFloat((float)Math.PI * 2f);

        private new float wobbleTo = Calc.Random.NextFloat((float)Math.PI * 2f);

        private new float wobbleEase;
        public new float animTimer;
        public new MTexture[] particleTextures =
        [
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(14, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(0, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7)
        ];

        public override void Update()
        {
            if (solid.Scene is null)
            {
                RemoveSelf();
                return;
            }
            Collidable = solid.Collidable;
            Visible = solid.Visible;
            Entity_Update();
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
            MoveTo(solid.Position - offset);
        }
        [MonoMod.MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
        public void Entity_Update()
        {
            throw new NotImplementedException("How? This should be relinked.");
        }
        [MonoMod.MonoModLinkTo("Monocle.Entity", "System.Void Render()")]
        public void Entity_Render()
        {
            throw new NotImplementedException("How? This should be relinked.");
        }
        bool Inside(Vector2 pos)
        {
            // check if pos is inside Grid.

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
            Rectangle tileBounds = solid.Collider.Bounds;
            var tiledata = (solid.Collider as Grid)!.Data;
            // multiply result by 2,
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
                foreach (DreamifierRenderer_Grid rects in Scene.Tracker.Entities[typeof(DreamifierRenderer_Grid)])
                {
                    foreach (var rec in rects.AltCollider)
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
    [Tracked]
    [TrackedAs(typeof(DreamBlock))]
    public class DreamifierRenderer_Hitbox : DreamBlock
    {

        List<(Vector2 from, Vector2 to)> WobbleList = [];
        List<Vector2> CornerList = [];

        Solid solid;

        public Color lineColor;
        public Color fillColor;
        public Color linecolorDeact;
        public Color fillColorDeact;

        public DreamifierRenderer_Hitbox(Vector2 position, int width, int height, Color line, Color block,
            Color linede, Color fillde, Solid solid) : base(position, width, height, null, false, false, false)
        {
            lineColor = line;
            fillColor = block;
            linecolorDeact = linede;
            fillColorDeact = fillde;
            DreamBlockConfig.Get(this).HighPriority();
            //vanilla uses 10
            //for footstep ripple priority
            SurfaceSoundPriority = 11;
            Renderer = null!;
            particles = null!;
            this.solid = solid;
        }

        Vector2 offset;
        DepthTracker Renderer;
        public override void Awake(Scene _scene)
        {
            base.Awake(_scene);
            solid.Add(Renderer = new DepthTracker(solid, this));

            var rect = Rectangle.Intersect(solid.Hitbox.Bounds, Hitbox.Bounds);
            Position = new(rect.X, rect.Y);
            Collider = new Hitbox(rect.Width, rect.Height);

            bool top = false, bottom = false, left = false, right = false;
            if (solid.Top == Top)
            {
                top = true;
                WobbleList.Add((Collider.TopLeft, Collider.TopRight));
            }
            if (solid.Bottom == Bottom)
            {
                bottom = true;
                WobbleList.Add((Collider.BottomRight, Collider.BottomLeft));
            }
            if (solid.Left == Left)
            {
                left = true;
                WobbleList.Add((Collider.BottomLeft, Collider.TopLeft));
            }
            if (solid.Right == Right)
            {
                right = true;
                WobbleList.Add((Collider.TopRight, Collider.BottomRight));
            }

            if (top && left)
            {
                CornerList.Add(Collider.TopLeft + new Vector2(1, 1));
            }
            if (bottom && left)
            {
                CornerList.Add(Collider.BottomLeft + new Vector2(1, -2));
            }
            if (bottom && right)
            {
                CornerList.Add(Collider.BottomRight + new Vector2(-2, -2));
            }
            if (top && right)
            {
                CornerList.Add(Collider.TopRight + new Vector2(-2, 1));
            }
            Setup();

            offset = solid.Position - Position;
        }
        new TilesetDreamBlock.DreamParticle[] particles;
        public new void Setup()
        {


            particles = new TilesetDreamBlock.DreamParticle[(int)(base.Width / 8f * (base.Height / 8f) * 0.7f)];
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Position = new Vector2(Calc.Random.NextFloat(base.Width), Calc.Random.NextFloat(base.Height));
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
            Renderer.RemoveSelf();
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
            Draw.Rect(shake.X + X, shake.Y + Y, Width, Height, playerHasDreamDash ? fillColor : fillColorDeact);
            Vector2 position = SceneAs<Level>().Camera.Position;
            for (int i = 0; i < particles.Length; i++)
            {
                int layer = particles[i].Layer;
                Vector2 position2 = particles[i].Position;
                position2 += position * (0.3f + 0.25f * layer);
                position2 = PutInside(position2);

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
                WobbleLine(Position + f, Position + t, off += 0.75f);
            }
            foreach (var r in CornerList)
            {
                Draw.Rect(Position + shake + r, 1, 1, playerHasDreamDash ? lineColor : linecolorDeact);
            }
        }
        private new void WobbleLine(Vector2 from, Vector2 to, float offset)
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
        private static new float Lerp(float a, float b, float percent)
        {
            return a + (b - a) * percent;
        }
        private static new float LineAmplitude(float seed, float index)
        {
            return (float)(Math.Sin((double)(seed + index / 16f) + Math.Sin(seed * 2f + index / 32f) * 6.2831854820251465) + 1.0) * 1.5f;
        }

        private new float wobbleFrom = Calc.Random.NextFloat((float)Math.PI * 2f);

        private new float wobbleTo = Calc.Random.NextFloat((float)Math.PI * 2f);

        private new float wobbleEase;
        public new float animTimer;
        public new MTexture[] particleTextures =
        [
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(14, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(0, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7)
        ];
        public override void Update()
        {
            if (solid.Scene is null)
            {
                RemoveSelf();
                return;
            }
            Collidable = solid.Collidable;
            Visible = solid.Visible;
            Entity_Update();
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
            MoveTo(solid.Position - offset);
        }
        [MonoMod.MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
        public void Entity_Update()
        {
            throw new NotImplementedException("How? This should be relinked.");
        }
        [MonoMod.MonoModLinkTo("Monocle.Entity", "System.Void Render()")]
        public void Entity_Render()
        {
            throw new NotImplementedException("How? This should be relinked.");
        }
    }
}