using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Reflection;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.ReverseHelper.Entities
{
    enum DashResultOverride
    {
        Rebound,
        NormalCollision,
        NormalOverride,
        Bounce,
        Ignore,
        DoNotOverride,
    }
    [CustomEntity("ReverseHelper/DashConductionJumpThru")]
    public class DashConductionJumpThru : Entity
    {
        public JumpThru? js;

        public DashConductionJumpThru(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            js = ReverseHelperExtern.VortexHelperModule.AttachedJumpThru.ctor(data, offset);
            if (js is not null)
            {
                var mov = js.Get<StaticMover>();
                var r=data.Enum<DashResultOverride>("overrideCollisionResult");
                DashCollision Func;
                if(r==DashResultOverride.DoNotOverride)
                {
                    Func=(Player p, Vector2 v) =>
                    mov.Platform?.OnDashCollide?.Invoke(p, v) ?? DashCollisionResults.NormalCollision;
                }
                else
                {
                    var rr=data.Enum<DashCollisionResults>("overrideCollisionResult");
                    Func = (Player p, Vector2 v) =>
                    {
                        mov.Platform?.OnDashCollide?.Invoke(p, v);
                        return rr;
                    };
                }
                js.OnDashCollide += Func;
            }
        }
        public override void Added(Scene scene)
        {
            if (js is not null)
            {
                scene.Add(js);
            }
            base.Added(scene);
        }
    }
    [CustomEntity("GravityHelper/ReverseHelper/DashConductionJumpThruUpsideDown",
        "ReverseHelper/DashConductionJumpThruUpsideDown = LegacyCtor")]
    public class DashConductionJumpThruUpsideDown : Entity
    {
        public JumpThru? js;
        public static DashConductionJumpThruUpsideDown LegacyCtor(Level level, LevelData levelData, Vector2 offset, EntityData data)
        {
            ReverseHelperExtern.GravityHelperModule.RequireGravityHelperHook();
            return new DashConductionJumpThruUpsideDown(data, offset);
        }
        public DashConductionJumpThruUpsideDown(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            try
            {
                data.Values.Add("attached", true);
            }
            catch { }

            js = ReverseHelperExtern.GravityHelperModule.UpsideDownJumpThru.ctor(data, offset);
            if (js is not null)
            {
                var mov = js.Get<StaticMover>();
                var r = data.Enum<DashResultOverride>("overrideCollisionResult");
                DashCollision Func;
                if (r == DashResultOverride.DoNotOverride)
                {
                    Func = (Player p, Vector2 v) =>
                    mov.Platform?.OnDashCollide?.Invoke(p, v) ?? DashCollisionResults.NormalCollision;
                }
                else
                {
                    var rr = data.Enum<DashCollisionResults>("overrideCollisionResult");
                    Func = (Player p, Vector2 v) =>
                    {
                        mov.Platform?.OnDashCollide?.Invoke(p, v);
                        return rr;
                    };
                }
                js.OnDashCollide += Func;
            }
        }
        public override void Added(Scene scene)
        {
            if (js is not null)
            {
                scene.Add(js);
            }
            base.Added(scene);
        }
    }
    [CustomEntity("ReverseHelper/DashConductionJumpThruSideways")]
    public class DashConductionJumpThruSideways : Entity
    {
        public Entity? js;
        public DashConductionJumpThruSideways(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            ReverseHelperExtern.MaddieHelpingHandModule.SidewaysJumpThru.activateHooks();

            js = ReverseHelperExtern.MaddieHelpingHandModule.AttachedSidewaysJumpThru.ctor(data, offset);
            if (js is not null)
            {
                var mov = js.Get<StaticMover>();
                var r = data.Enum<DashResultOverride>("overrideCollisionResult");
                DashCollision Func;
                if (r == DashResultOverride.DoNotOverride)
                {
                    Func = (Player p, Vector2 v) =>
                    mov.Platform?.OnDashCollide?.Invoke(p, v) ?? DashCollisionResults.NormalCollision;
                }
                else
                {
                    var rr = data.Enum<DashCollisionResults>("overrideCollisionResult");
                    Func = (Player p, Vector2 v) =>
                    {
                        mov.Platform?.OnDashCollide?.Invoke(p, v);
                        return rr;
                    };
                }

                ReverseHelperExtern.MaddieHelpingHandModule.AttachedSidewaysJumpThru.SetIfNull_OnDashCollide(js,Func);
            }

        }
        public override void Added(Scene scene)
        {
            if (js is not null)
            {
                scene.Add(js);
            }
            base.Added(scene);
        }
    }
}