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
    [CustomEntity("ReverseHelper/DashConductionJumpThru = Ctor")]
    public static class DashConductionJumpThru
    {
        public static JumpThru Ctor(Level level, LevelData levelData, Vector2 offset, EntityData data)
        {
            JumpThru? js = null ;
            if (Level.EntityLoaders.TryGetValue("VortexHelper/AttachedJumpThru", out var entityLoader))
            {
                js = (JumpThru?)entityLoader(level, levelData, offset, data);
            }
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
            return js;
        }
    }
    [CustomEntity("GravityHelper/ReverseHelper/DashConductionJumpThruUpsideDown = Ctor")]
    public static class DashConductionJumpThruUpsideDown
    {
        public static JumpThru Ctor(Level level,LevelData levelData, Vector2 offset,EntityData data)
        {
            JumpThru? js = null ;
            data.Values.TryAdd("attached", true);
            if (Level.EntityLoaders.TryGetValue("GravityHelper/UpsideDownJumpThru", out var entityLoader))
            {
                js = (JumpThru?)entityLoader(level, levelData, offset, data);
            }
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
            return js;
        }
    }
    [CustomEntity("ReverseHelper/DashConductionJumpThruSideways = Ctor")]
    public class DashConductionJumpThruSideways : Entity
    {
        public static Entity Ctor(Level level, LevelData levelData, Vector2 offset, EntityData data)
        {
            Entity? js = null;
            ReverseHelperExtern.MaddieHelpingHandModule.SidewaysJumpThru.activateHooks();

            if (Level.EntityLoaders.TryGetValue("MaxHelpingHand/AttachedSidewaysJumpThru", out var entityLoader))
            {
                js = entityLoader(level, levelData, offset, data);
            }
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
            return js;
        }
    }
}