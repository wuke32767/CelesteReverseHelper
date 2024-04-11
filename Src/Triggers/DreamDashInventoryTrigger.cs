using Celeste.Mod.Entities;
using Celeste.Mod.ReverseHelper.Entities;
using Celeste.Mod.ReverseHelper.Libraries;
using Microsoft.Xna.Framework;
using Monocle;
using System.Linq;

namespace Celeste.Mod.ReverseHelper.Triggers
{
    [CustomEntity("ReverseHelper/DreamDashInventoryTrigger")]
    public class DreamDashInventoryTrigger : Trigger
    {

        public DreamDashInventoryTrigger(EntityData e, Vector2 offset, bool onlyFire, bool onlyIce) : base(e, offset)
        {
            this.onlyDisable = onlyFire;
            this.onlyEnable = onlyIce;
            this.persistent = persistent = true;
            base.Add(new DreamToggleListener((OnChangeMode)));
            base.Depth = 2000;
        }

        // Token: 0x0600164F RID: 5711 RVA: 0x0005C3BA File Offset: 0x0005A5BA
        public DreamDashInventoryTrigger(EntityData data, Vector2 offset) : this(data, offset, data.Bool("onlyEnable", false), data.Bool("onlyDisable", false))
        {
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            OnPlayer(player);
        }

        public override void Update()
        {
            base.Update();
        }



        // Token: 0x0600164F RID: 5711 RVA: 0x0005C3BA File Offset: 0x0005A5BA

        // Token: 0x06001650 RID: 5712 RVA: 0x0005C3F2 File Offset: 0x0005A5F2

        public override void Added(Scene scene)
        {
            base.Added(scene);
            enableMode = (ReverseHelperModule.playerHasDreamDash);
        }

        // Token: 0x06001651 RID: 5713 RVA: 0x0005C416 File Offset: 0x0005A616
        private void OnChangeMode(bool enabled)
        {
            enableMode = (ReverseHelperModule.playerHasDreamDash);
        }

        // Token: 0x06001652 RID: 5714 RVA: 0x0005C42C File Offset: 0x0005A62C


        // Token: 0x06001653 RID: 5715 RVA: 0x0005C524 File Offset: 0x0005A724

        private void OnPlayer(Player player)
        {
            if (Usable && cooldownTimer <= 0f)
            {
                playSounds = true;
                Level level = base.SceneAs<Level>();

                //if (level.CoreMode == Session.CoreModes.Cold)
                //{
                //    level.CoreMode = Session.CoreModes.Hot;
                //}
                //else
                //{
                //    level.CoreMode = Session.CoreModes.Cold;
                //}
                //if (persistent)
                //{
                //    level.Session.CoreMode = level.CoreMode;
                //}

                ReverseHelperModule.playerHasDreamDash = !ReverseHelperModule.playerHasDreamDash;

                DreamToggleListener.ImmediateUpdate();
            }
        }

        // Token: 0x06001654 RID: 5716 RVA: 0x0005C5BB File Offset: 0x0005A7BB


        // Token: 0x17000340 RID: 832
        // (get) Token: 0x06001655 RID: 5717 RVA: 0x0005C5E2 File Offset: 0x0005A7E2
        private bool Usable
        {

            get
            {
                return (!onlyDisable || !enableMode) && (!onlyEnable || enableMode);
            }
        }

        // Token: 0x04000F79 RID: 3961
        private const float Cooldown = 1f;

        // Token: 0x04000F7A RID: 3962
        private bool enableMode;

        // Token: 0x04000F7B RID: 3963
        private const float cooldownTimer = 0;

        // Token: 0x04000F7C RID: 3964
        private bool onlyDisable;

        // Token: 0x04000F7D RID: 3965
        private bool onlyEnable;

        // Token: 0x04000F7E RID: 3966
        private bool persistent;

        // Token: 0x04000F7F RID: 3967
        private bool playSounds;
    }
}