using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [CustomEntity("ReverseHelper/DashTeleportationRefill")]
    [SourceGen.Loader.LazyLoad]
    public class DashTeleportationRefill : Refill
    {
        Vector2?[] sequence;
        int progress;
        float refillTimer;
        public DashTeleportationRefill(EntityData e, Vector2 offset)
            : base(e.Position + offset, false, e.Bool("oneUse"))
        {
            refillTimer = e.Float("refillTime", 2.5f);
            sequence = e.Nodes?.Select(x => (Vector2?)x + offset).ToArray() ?? [Center];
            progress = sequence.Length;
            Get<PlayerCollider>().OnCollide = p =>
            {
                progress = 0;
                Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;
                Add(new Coroutine(RefillRoutine(p)));
                respawnTimer = refillTimer;

                next.GetOrCreateValue(p).Push(this);
            };

            var text = "objects/refillTwo/";
            Remove(outline, sprite, flash);
            Add(outline = new Image(GFX.Game[text + "outline"]));
            outline.CenterOrigin();
            outline.Visible = false;
            Add(sprite = new Sprite(GFX.Game, text + "idle"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(flash = new Sprite(GFX.Game, text + "flash"));
            flash.Add("flash", "", 0.05f);
            flash.OnFinish = delegate
            {
                flash.Visible = false;
            };
            flash.CenterOrigin();
        }
        public override void Update()
        {
            this.Entity_Update();
            if (respawnTimer > 0f)
            {
                if (progress == sequence.Length)
                {
                    respawnTimer -= Engine.DeltaTime;
                    if (respawnTimer <= 0f)
                    {
                        Respawn();
                    }
                }
            }
            else if (Scene.OnInterval(0.1f))
            {
                level.ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
            }

            UpdateY();
            light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
            bloom.Alpha = light.Alpha * 0.8f;
            if (Scene.OnInterval(2f) && sprite.Visible)
            {
                flash.Play("flash", restart: true);
                flash.Visible = true;
            }
        }
        static ConditionalWeakTable<Player, Stack<DashTeleportationRefill>> next = new();

        [SourceGen.Loader.Load]
        public static void Load()
        {
            On.Celeste.Player.DashBegin += Player_DashBegin;
        }

        private static void Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
        {
            var l = next.GetOrCreateValue(self);
            if (l.Count > 0)
            {
                var refill = l.Peek();
                self.Position = refill.sequence[refill.progress] ?? self.Position;
                refill.progress++;
                if (refill.progress == refill.sequence.Length)
                {
                    l.Pop();
                }
            }
            orig(self);
        }

        [SourceGen.Loader.Unload]
        public static void Unload()
        {
            On.Celeste.Player.DashBegin -= Player_DashBegin;
        }

    }
}