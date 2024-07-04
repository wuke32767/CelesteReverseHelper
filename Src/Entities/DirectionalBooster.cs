using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [CustomEntity("ReverseHelper/DirectionalBooster")]
    public class DirectionalBooster : Booster
    {
        [Flags]
        public enum Direction
        {
            right = 1,
            rightdown = 2,
            down = 4,
            leftdown = 8,
            left = 16,
            leftup = 32,
            up = 64,
            rightup = 128,
        }
        public bool[] directionList;
        public Direction dir;
        public Direction @default;
        public DirectionalComponent component;

        public DirectionalBooster(EntityData e, Vector2 offset) : base(e, offset)
        {

            dir =
                Enum.GetNames<Direction>()
                .Select(s =>
                {
                    if (e.Bool(s))
                    {
                        return Enum.Parse<Direction>(s);
                    }
                    return (Direction)0;
                })
                .Aggregate((a, b) => a | b);
            @default = e.Enum<Direction>("default");

            Add(component = new(this));
            if (!dir.HasFlag(@default))
            {
                //throw new ArgumentException("Can't dash into this direction!", "default");
            }
        }
        static ILHook il;
        //[SourceGen.Loader.Load]
        public static void Load()
        {
            il = typeof(Player)
                 .GetMethod("DashCoroutine", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                 .GetStateMachineTarget()!
                 .ILState((1, il =>
                 {

                 }
            ));
        }
        //[SourceGen.Loader.Unload]
        public static void Unload()
        {
            il.Dispose();
        }
        public override void Update()
        {
            base.Update();
            component.pos = sprite.Position;
            component.Visible = sprite.Visible;
        }
        public class DirectionalComponent(DirectionalBooster db)
            : Component(true, true)
        {
            public Vector2 pos = Vector2.Zero;
            public DirectionalBooster Booster => db;
            float time = 0.6f;
            static List<MTexture> image = default!;
            [SourceGen.Loader.LoadContent]
            public static void LoadContent()
            {
                image = GFX.Game.GetAtlasSubtextures("util/dasharrow/dasharrow");
            }
            public override void Update()
            {
                if (Entity is Player)
                {
                    time = Calc.Approach(time, 1, Engine.DeltaTime * 3);
                }
                else
                {
                    time = 0.6f;
                }
                base.Update();
            }
            public override void Render()
            {
                base.Render();
                float progress = Ease.Linear(time);
                var center = Entity.Center + pos;
                Vector2 from = Vector2.UnitX;
                for (int i = 0; i < 8; i++)
                {
                    var cur = (Direction)(1 << i);
                    Vector2 position = center + from * MathHelper.Lerp(0, 24 * MathF.Sqrt(2), progress);
                    if (db.dir.HasFlag(cur))
                    {
                        image[i].DrawCentered(position, db.@default == cur ? Color.Orange : Color.White, progress);
                    }
                    else if (db.@default == cur)
                    {
                        image[i].DrawCentered(position, Color.Blue, progress);
                    }
                    from = from.Rotate((45.0f).ToRad()); // precision moment
                }
            }
        }
    }
}