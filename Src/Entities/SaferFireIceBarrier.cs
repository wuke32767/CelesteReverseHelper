using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.ReverseHelper.Entities
{
    // Token: 0x02000278 RID: 632
    [CustomEntity("ReverseHelper/SaferFireIceBarrier")]
    [Tracked]
    public class SaferFireIceBarrier : Solid
    {
        bool topdanger;
        bool topsafer;
        bool toprestore = false;
        int safegrace = 0;
        bool dangercoroutine = false;
        bool danger { get => dangercoroutine && safegrace == 0; }
        int dangergrace = 0;
        bool framed = false;
        bool ice;
        bool cold = false, hot = false, none = false;
        // Token: 0x060012D7 RID: 4823 RVA: 0x00048F70 File Offset: 0x00047170
        public SaferFireIceBarrier(Vector2 position, float width, float height, bool icelook, bool c, bool h, bool n, bool toprestore, bool topsafer, bool topdanger) : base(position, width, height, false)
        {
            Tag = Tags.TransitionUpdate;
            Collider = new Hitbox(width, height, 0f, 0f);
            Add(new PlayerCollider(OnPlayer, null, null));
            Add(new CoreModeListener(OnChangeMode));
            ice = icelook;
            if (ice)
            {
                Add(Lava = new LavaRect(width, height, 2));
                Lava.UpdateMultiplier = 0f;
                Lava.SurfaceColor = Calc.HexToColor("a6fff4");
                Lava.EdgeColor = Calc.HexToColor("6cd6eb");
                Lava.CenterColor = Calc.HexToColor("4ca8d6");
                Lava.SmallWaveAmplitude = 1f;
                Lava.BigWaveAmplitude = 1f;
                Lava.CurveAmplitude = 1f;
                Lava.Spikey = 3f;
            }
            else
            {
                Add(Lava = new LavaRect(width, height, 4));
                Lava.SurfaceColor = RisingLava.Hot[0];
                Lava.EdgeColor = RisingLava.Hot[1];
                Lava.CenterColor = RisingLava.Hot[2];
                Lava.SmallWaveAmplitude = 2f;
                Lava.BigWaveAmplitude = 1f;
                Lava.CurveAmplitude = 1f;
            }
            Depth = -8500;
            Add(idleSfx = new SoundSource());
            idleSfx.Position = new Vector2(Width, Height) / 2f;
            cold = c;
            hot = h;
            none = n;
            OnDashCollide = (p, v) =>
            {
                if (this.topsafer)
                {
                    if (p.Top >= Bottom || p.Bottom <= Top)
                    {
                        return DashCollisionResults.NormalCollision;
                    }
                }
                return DashCollisionResults.Ignore;
            };
            this.toprestore = toprestore;
            this.topsafer = topsafer;
            this.topdanger = topdanger;

            //OnCollide = dangerous;
        }
        void playertouch(Vector2 v)
        {
            if (framed)
            {
                return;
            }
            if (dangercoroutine == false)
            {
                dangercoroutine = true;
                if (v.X == 0)
                {
                    if (topdanger)
                    {
                        Engine.Scene.Tracker.GetEntity<Player>().Die(-v);
                    }
                    else if (topsafer)
                    {
                        dangergrace = 4;
                        safegrace = 2;
                    }
                    else
                    {
                        dangergrace = 2;
                    }
                }
                else
                {
                    dangergrace = 2;
                }
            }
            else if (danger)
            {
                Engine.Scene.Tracker.GetEntity<Player>().Die(-v);
            }
            framed = true;
        }
        // Token: 0x060012D8 RID: 4824 RVA: 0x000490A9 File Offset: 0x000472A9
        public SaferFireIceBarrier(EntityData e, Vector2 offset)
            : this(e.Position + offset, e.Width, e.Height, e.Bool("iceBlock"),
                  e.Bool("cold"), e.Bool("hot"), e.Bool("none"),
                  e.Bool("topRefillDash", true), e.Bool("topSafer"), e.Bool("topDangerous"))
        {
        }

        // Token: 0x060012D9 RID: 4825 RVA: 0x000490CC File Offset: 0x000472CC
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!toprestore)
            {
                var s = new Spikes(new(X, Y + 2), (int)Width, Spikes.Directions.Up, "");
                var t = new Spikes(new(X, Y + Height - 2), (int)Width, Spikes.Directions.Down, "");
                s.Visible = false;
                t.Visible = false;
                scene.Add(s, t);
            }
            Collidable = collideable(SceneAs<Level>().CoreMode);
            if (Collidable)
            {
                idleSfx.Play("event:/env/local/09_core/lavagate_idle", null, 0f);
            }
        }
        bool collideable(Session.CoreModes mode)
        {
            return mode == Session.CoreModes.Hot && hot
                    || mode == Session.CoreModes.None && none
                    || mode == Session.CoreModes.Cold && cold;
        }
        // Token: 0x060012DA RID: 4826 RVA: 0x00049168 File Offset: 0x00047368
        private void OnChangeMode(Session.CoreModes mode)
        {
            Collidable = collideable(mode);
            if (!Collidable)
            {
                Level level = SceneAs<Level>();
                Vector2 center = Center;
                int num = 0;
                while (num < Width)
                {
                    int num2 = 0;
                    while (num2 < Height)
                    {
                        Vector2 vector = Position + new Vector2(num + 2, num2 + 2) + Calc.Random.Range(-Vector2.One * 2f, Vector2.One * 2f);
                        level.Particles.Emit(P_Deactivate, vector, (vector - center).Angle());
                        num2 += 4;
                    }
                    num += 4;
                }
                idleSfx.Stop(true);
                return;
            }
            idleSfx.Play("event:/env/local/09_core/lavagate_idle", null, 0f);
        }

        // Token: 0x060012DB RID: 4827 RVA: 0x00033690 File Offset: 0x00031890
        private void OnPlayer(Player player)
        {
            player.Die((player.Center - Center).SafeNormalize(), false, true);
        }

        // Token: 0x060012DC RID: 4828 RVA: 0x00049265 File Offset: 0x00047465
        public override void Update()
        {
            if ((Scene as Level)!.Transitioning)
            {
                if (idleSfx != null)
                {
                    idleSfx.UpdateSfxPosition();
                }
                return;
            }
            base.Update();
            var player = Engine.Scene.Tracker.GetEntity<Player>();
            if ((player?.IsRiding(this) ?? false))
            {
                Vector2 to = Vector2.Zero;
                if (player.Left >= Right)
                {
                    to = new(1, 0);
                }
                else if (player.Top >= Bottom)
                {
                    to = new(0, 1);
                }
                else if (player.Right <= Left)
                {
                    to = new(-1, 0);
                }
                else
                {
                    to = new(0, -1);
                }
                playertouch(to);
            }

            if (dangergrace == 0)
            {
                dangercoroutine = false;
            }
            else
            {
                dangergrace -= 1;
            }
            if (safegrace == 0)
            {

            }
            else
            {
                safegrace -= 1;
            }
            framed = false;
            timer += Engine.DeltaTime;
        }
        float timer = 0;
        // Token: 0x060012DD RID: 4829 RVA: 0x000336B1 File Offset: 0x000318B1
        public override void Render()
        {
            if (Collidable)
            {
                base.Render();
            }
        }

        // Token: 0x04000C8F RID: 3215
        public ParticleType P_Deactivate
        {
            get
            {
                if (ice)
                {
                    return IceBlock.P_Deactivate;
                }
                return FireBarrier.P_Deactivate;
            }
        }
        static ILHook? orig_Update;
        [SourceGen.Loader.Load]

        public static void Load()
        {
            orig_Update = new ILHook(typeof(Player).GetMethod("orig_Update", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), Player_Update);
            //IL.Celeste.Player.Update += Player_Update;
        }

        private static void Player_Update(ILContext il)
        {
            ILCursor ic = new(il);
            while (ic.TryGotoNext(MoveType.After, i => i.MatchLdfld(out var v) && v.Name == "onCollideH",
                i => i.MatchLdnull(),
                i => i.MatchCallOrCallvirt(out var v) && v.Name == "MoveH"))
            {
                ic.Emit(OpCodes.Ldarg_0);
                ic.EmitDelegate(nameless1);
                static void nameless1(Player p)
                {
                    if (p.CollideFirst<Solid>(p.Position + new Vector2(Math.Sign(p.Speed.X), 0)) is SaferFireIceBarrier safe)
                    {
                        safe.playertouch(new Vector2(Math.Sign(p.Speed.X), 0));
                    }
                };
            }
            ic.Index = 0;
            while (ic.TryGotoNext(MoveType.After, i => i.MatchLdfld(out var v) && v.Name == "onCollideV",
                i => i.MatchLdnull(),
                i => i.MatchCallOrCallvirt(out var v) && v.Name == "MoveV"))
            {

                ic.Emit(OpCodes.Ldarg_0);
                ic.EmitDelegate(nameless2);
                static void nameless2(Player p)
                {
                    if (p.CollideFirst<Solid>(p.Position + new Vector2(0, Math.Sign(p.Speed.Y))) is SaferFireIceBarrier safe)
                    {
                        safe.playertouch(new Vector2(0, Math.Sign(p.Speed.Y)));
                    }
                };
            }
        }

        [SourceGen.Loader.Unload]
        public static void Unload()
        {
            orig_Update?.Dispose();
        }
        // Token: 0x04000C90 RID: 3216
        private LavaRect Lava;

        // Token: 0x04000C91 RID: 3217
        //private Solid solid;

        // Token: 0x04000C92 RID: 3218
        private SoundSource idleSfx;
    }
}
