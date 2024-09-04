using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Linq;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [CustomEntity("ReverseHelper/TilesetDepthSetter")]
    [Tracked]
    public class TilesetDepthSetter(Vector2 position, int width, int height, int id, int depths, bool fg, bool bg,int code_side) : Entity()
    {
        StaticMover staticMover = new StaticMover();
        TileGrid mt;
        int ID => id;
        int CodeID => code_side;
        public override void Added(Scene scene)
        {
            if (fg && bg)
            {
                throw new ArgumentException("cant set fg and bg at the same time");
            }
            if (fg == bg)
            {
                Logger.Log(LogLevel.Warn, nameof(ReverseHelper), "empty Tileset Depth Setter");
            }
            base.Added(scene);
            if (scene.Tracker.GetEntities<TilesetDepthSetter>()
                .OfType<TilesetDepthSetter>().Any(x => x != this && x.ID == ID && x.CodeID == CodeID))
            {
                RemoveSelf();
                return;
            }
            Tag |= Tags.Global;
            if (scene.Tracker.GetEntities<TilesetDepthSetter>()
                .OfType<TilesetDepthSetter>().FirstOrDefault(x => x != this && x.Depth == depths) is { } dep)
            {
                dep.ExtendRange(position, width, height, fg, bg);
                return;
            }

            var maps = SceneAs<Level>().SolidTiles;
            var grids = maps.Tiles;
            mt = new(grids.TileWidth, grids.TileHeight, grids.TilesX, grids.TilesY);
            Position = maps.Position;
            mt.Position = grids.Position;
            Add(mt);
            Add(staticMover);
            maps.staticMovers.Add(staticMover);
            ExtendRange(position, width, height, fg, bg);
        }
        public void ExtendRange(Vector2 position, int width, int height, bool fg = true, bool bg = true, bool collider = false)
        {
            var maps = SceneAs<Level>().SolidTiles;
            TileGrid grids = null!;

            position -= maps.TopLeft;
            int x = (int)position.X;
            int y = (int)position.Y;

            x /= 8;
            y /= 8;
            width /= 8;
            height /= 8;
            var run = () =>
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        mt.Tiles[x + i, y + j] ??= grids.Tiles[x + i, y + j];
                        grids.Tiles[x + i, y + j] = null;
                    }
                }
            };
            
            if (fg)
            {
                grids = maps.Tiles;
                run();
            }
            if (bg)
            {
                grids = SceneAs<Level>().BgTiles.Tiles;
                run();
            }
            //if(collider)
            //{
            //    for (int i = 0; i < width; i++)
            //    {
            //        for (int j = 0; j < height; j++)
            //        {
            //            mt.Tiles[x + i, y + j] ??= grids.Tiles[x + i, y + j];
            //        }
            //    }
            //}
        }
        public TilesetDepthSetter(EntityData e, Vector2 offset)
            : this(e.Position + offset, e.Width, e.Height, e.ID,
                  e.Int("depth"), e.Bool("fg"), e.Bool("bg"), 0)
        {
        }
    }
}