using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.ReverseHelper.Entities
{

    // Token: 0x02000278 RID: 632
    [CustomEntity("ReverseHelper/SaferFireIceBarrier")]
    public class SaferFireIceBarrier : Solid
    {
        bool danger = false;
        bool dangergrace = false;
        bool ice;
        bool cold = false, hot = false, none = false;
        // Token: 0x060012D7 RID: 4823 RVA: 0x00048F70 File Offset: 0x00047170
        [MethodImpl(MethodImplOptions.NoInlining)]
        public SaferFireIceBarrier(Vector2 position, float width, float height, bool icelook, bool c, bool h, bool n) : base(position, width, height, false)
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
            OnCollide = v =>
            {
                if (danger == false)
                {
                    danger = true;
                    dangergrace = true;
                }
                else
                {
                    Engine.Scene.Tracker.GetEntity<Player>().Die(-v);
                }
            };
        }

        // Token: 0x060012D8 RID: 4824 RVA: 0x000490A9 File Offset: 0x000472A9
        [MethodImpl(MethodImplOptions.NoInlining)]
        public SaferFireIceBarrier(EntityData e, Vector2 offset)
            : this(e.Position + offset, e.Width, e.Height, e.Bool("iceBlock"),
                  e.Bool("cold"), e.Bool("hot"), e.Bool("none"))
        {
        }

        // Token: 0x060012D9 RID: 4825 RVA: 0x000490CC File Offset: 0x000472CC
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Added(Scene scene)
        {
            base.Added(scene);

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
        [MethodImpl(MethodImplOptions.NoInlining)]
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
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void OnPlayer(Player player)
        {
            player.Die((player.Center - Center).SafeNormalize(), false, true);
        }

        // Token: 0x060012DC RID: 4828 RVA: 0x00049265 File Offset: 0x00047465
        [MethodImpl(MethodImplOptions.NoInlining)]
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
            if (!dangergrace)
            {
                danger = false;
            }
            dangergrace = false;
        }

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

        // Token: 0x04000C90 RID: 3216
        private LavaRect Lava;

        // Token: 0x04000C91 RID: 3217
        //private Solid solid;

        // Token: 0x04000C92 RID: 3218
        private SoundSource idleSfx;
    }
}
