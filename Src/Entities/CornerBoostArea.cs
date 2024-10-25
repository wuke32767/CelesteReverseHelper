using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [CustomEntity("ReverseHelper/CornerBoostArea")]
    [SourceGen.Loader.Dependency(typeof(CBAreaComponent))]
    public class CornerBoostArea : Entity
    {
        public PlayerCollider playerCollider;
        public readonly int totaltimes;
        public readonly float cooldown;
        public readonly int times;
        public int count_totaltimes;
        public float count_cooldown;
        public int count_times;

        public bool cooldownOnExit;
        public bool onlyRCB;
        public bool cornerhyper;

        public Color mycol;
        public Color Line;

        public bool legacy;

        public CornerBoostArea(Vector2 position, int width, int height, int depth, int totaltimes, float cooldown, int times, bool cooldownOnExit, bool onlyRCB, bool cornerhyper, Color mycol, Color line, bool transparent, Color old, bool legacy) : base(position)
        {
            Collider = new Hitbox(width, height);
            Add(playerCollider = new PlayerCollider((Player p) =>
            {
                var com = p.Get<CBAreaComponent>();
                if (com is null)
                {
                    p.Add(com = new CBAreaComponent());
                }
                com.src = this;
            }));
            Depth = depth;
            this.totaltimes = totaltimes;
            this.cooldown = cooldown;
            this.times = times;
            this.cooldownOnExit = cooldownOnExit;
            this.onlyRCB = onlyRCB;
            this.cornerhyper = cornerhyper;
            this.legacy = legacy;
            if (legacy)
            {
                this.mycol = old * 0.125f;
                Line = old * 0.625f;
            }
            else
            {
                if (transparent)
                {
                    line *= 0.625f;
                    mycol *= 0.125f;
                }
                Line = line;
                this.mycol = mycol;
            }
        }

        public CornerBoostArea(EntityData e, Vector2 offset) : this(e.Position + offset, e.Width, e.Height,
            e.Int("depth", 1), e.Int("totalTimes", -1), e.Float("cooldown", 0), e.Int("timesBeforeExit", -1),
            e.Bool("cooldownOnExit"), e.Bool("onlyRCB"), e.Bool("cornerHyper", false),
            e.HexaColor("fillColor", Color.White), e.HexaColor("lineColor", Color.White), e.Bool("legacyTransparent", true),
            e.HexColor("color"), e.Attr("color", null) is not null)
        {
        }

        public override void Render()
        {
            if (Scene is Level level)
            {
                Camera camera = level.Camera;
                if (!(Right < camera.Left || Left > camera.Right || Bottom < camera.Top || Top > camera.Bottom))
                {
                    Draw.HollowRect(Collider, Line);
                    if (Collider.Width - 2 > 0 && Collider.Height - 2 > 0)
                    {
                        Draw.Rect(Collider.AbsoluteLeft + 1, Collider.AbsoluteTop + 1, Collider.Width - 2, Collider.Height - 2, mycol);
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();
            count_cooldown -= Engine.DeltaTime;
            count_cooldown = Math.Max(count_cooldown, 0);
        }
    }
    [SourceGen.Loader.LazyLoad]
    public class CBAreaComponent : Component
    {
        public override void Update()
        {
            base.Update();
            if (!(Entity as Player)!.CollideCheck(src))
            {
                src = null;
            }
        }
        
        public CornerBoostArea? _src;

        public CornerBoostArea? src
        {
            get => _src;
            set
            {
                if (src != value)
                {
                    if (_src is not null)
                    {
                        _src.count_times = 0;
                    }
                    if (src?.cooldownOnExit ?? false)
                    {
                        src.count_cooldown = 0;
                    }
                    Active = value is not null;
                    _src = value;
                }
            }
        }

        public CBAreaComponent() : base(false, false)
        {
        }
        [SourceGen.Loader.Load]
        public static void Load()
        {
            On.Celeste.Player.Update += Player_Update;
            On.Celeste.Player.Jump += Player_Jump;
            On.Celeste.Player.SuperJump += Player_SuperJump;
        }

        private static void Player_SuperJump(On.Celeste.Player.orig_SuperJump orig, Player self)
        {
            hyperjmped = true;
            orig(self);
        }

        public static bool jmped = false;
        public static bool hyperjmped = false;

        private static void Player_Jump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx)
        {
            jmped = true;
            orig(self, particles, playSfx);
        }

        [SourceGen.Loader.Unload]
        public static void Unload()
        {
            On.Celeste.Player.Update -= Player_Update;
            On.Celeste.Player.Jump -= Player_Jump;
            On.Celeste.Player.SuperJump -= Player_SuperJump;
        }

        private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            var spd = self.Speed.X;
            orig(self);
            var com = self.Get<CBAreaComponent>();
            if (com is not null && com.Active)
            {
                if (hyperjmped)
                {
                    jmped = true;
                    if (com.src!.cornerhyper)
                    {
                        self.Speed.X = spd;
                    }
                }
                if (!jmped)
                {
                    if (Input.Jump.Pressed
                        && (TalkComponent.PlayerOver == null || !Input.Talk.Pressed)
                        && Input.GrabCheck && self.Holding is null
                        && self.Stamina > 0f)
                    {
                        if (com.TryCB(self))
                        {
                            self.onCollideH(new CollisionData());
                            self.ClimbJump();
                            if (self.StateMachine.State == Player.StDash || self.StateMachine.State == Player.StRedDash)
                            {
                                self.StateMachine.ForceState(Player.StNormal);
                            }
                            if (Math.Sign(self.Speed.X) == (int)self.Facing)
                            {
                                self.Speed.X = 0;
                            }
                        }
                    }
                }
            }
            jmped = false;
            hyperjmped = false;
        }

        private bool TryCB(Player self)
        {
            if (src!.count_cooldown == 0 && src.count_times != src.times && src.count_totaltimes != src.totaltimes)
            {
                if (src.onlyRCB != true || Math.Sign(self.Speed.X) == -(int)self.Facing)
                {
                    src.count_cooldown = src.cooldown;
                    src.count_times += 1;
                    src.count_totaltimes += 1;

                    return true;
                }
            }
            return false;
        }
    }
}