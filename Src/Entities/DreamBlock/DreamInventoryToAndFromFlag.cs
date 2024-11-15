using Microsoft.Xna.Framework;

namespace Celeste.Mod.ReverseHelper.Entities
{
    [SourceGen.Loader.Preload("ReverseHelper/DreamDashToAndFromFlag")]
    public partial class DreamInventoryToAndFromFlag(string flags, int version) : Entity()
    {
        public string Flags => flags;

        Session session => SceneAs<Level>().Session;
        public DreamInventoryToAndFromFlag(EntityData e, Vector2 offset) : this(e.Attr("flag", "PlayerHasDreamDash"), e.Int("version", 1))
        {
        }
        bool working = false;
        public override void Added(Scene scene)
        {
            base.Added(scene);
            AddTag(Tags.Global);
            if (Scene.Entities.entities.OfType<DreamInventoryToAndFromFlag>().Where(x => x != this).Any())
            {
                RemoveSelf();
                return;
            }
            if(working)
            {
                return;
            }
            if (version == 3)
            {
                bool old;
                session.SetFlag(flags, old = ReverseHelperModule.playerHasDreamDashBetter(this));
                Add(new DreamToggleListener(has =>
                {
                    if (session.GetFlag(flags) != has)
                    {
                        session.SetFlag(flags, has);
                    }
                }));
                PostUpdate += _self =>
                {
                    var self = (_self as DreamInventoryToAndFromFlag)!;
                    var @new = self.session.GetFlag(self.Flags);
                    if (old != @new)
                    {
                        ReverseHelperModule.playerHasDreamDashBetter(self) = old = @new;
                        DreamToggleListener.ImmediateUpdate(self.Scene);
                    }
                };
            }
            else if (version == 2)
            {
                On.Celeste.Session.GetFlag += Session_GetFlagv2;
                On.Celeste.Session.SetFlag += Session_SetFlagv2;
            }
            else if (version == 1)
            {
                session.SetFlag(flags, ReverseHelperModule.playerHasDreamDashBetter(this));
                Add(new DreamToggleListener(has =>
                {
                    if (session.GetFlag(flags) != has)
                    {
                        session.SetFlag(flags, has);
                    }
                }));
                On.Celeste.Session.SetFlag += Session_SetFlag;
            }
            else
            {
                throw new InvalidOperationException("version should between 1 and 3");
            }
            working = true;
        }

        private void Session_SetFlagv2(On.Celeste.Session.orig_SetFlag orig, Session self, string flag, bool setTo)
        {
            orig(self, flag, setTo);
            if (flag == flags)
            {
                ReverseHelperModule.playerHasDreamDashBetter(this) = setTo;
                DreamToggleListener.ImmediateUpdate(Scene);
            }
        }

        private bool Session_GetFlagv2(On.Celeste.Session.orig_GetFlag orig, Session self, string flag)
        {
            var o = orig(self, flag);
            if (flag == flags)
            {
                var dd = ReverseHelperModule.playerHasDreamDashBetter(this);
                if (o != dd)
                {
                    if (dd)
                    {
                        self.Flags.Add(flag);
                    }
                    else
                    {
                        self.Flags.Remove(flag);
                    }
                    // ↓ stack overflow
                    // self.SetFlag(flag, dd);
                }
                return dd;
            }
            return o;
        }

        int impossible = 5;

        private void Session_SetFlag(On.Celeste.Session.orig_SetFlag orig, Session self, string flag, bool setTo)
        {
            impossible--;
            try
            {
                orig(self, flag, setTo);
                if (impossible == 0)
                {
                    //unreachable
                    Logger.Log(LogLevel.Error, nameof(ReverseHelper),
                        """

                    !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                           An unexpected exception was thrown.       
                     A real StackOverflow exception can't be catched,
                         so it's prevented and thrown manually.
                    !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    """);
                    static void throwhelper()
                    {
                        throw new StackOverflowException();
                    }
                    throwhelper();
                }
                if (flag == flags)
                {
                    ReverseHelperModule.playerHasDreamDashBetter(this) = setTo;
                    DreamToggleListener.ImmediateUpdate(Scene);
                }
            }
            finally
            {
                impossible++;
            }
        }
        void Unload()
        {
            if (!working) return;
            working = false;
            if (version == 2)
            {
                On.Celeste.Session.GetFlag -= Session_GetFlagv2;
                On.Celeste.Session.SetFlag -= Session_SetFlagv2;
            }
            else if (version == 1)
            {
                On.Celeste.Session.SetFlag -= Session_SetFlag;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Unload();
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Unload();
        }
    }
}