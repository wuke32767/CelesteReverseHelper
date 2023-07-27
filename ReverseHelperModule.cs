using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Celeste.Mod.ReverseHelper.Entities;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using YamlDotNet.Serialization;

namespace Celeste.Mod.ReverseHelper 
{
    public class ReverseHelperModule : EverestModule 
    {

        public static ReverseHelperModule Instance;
        public static int AnotherPurpleBoosterDashState;
        public static int AnotherPurpleBoosterState;
        public ReverseHelperModule() 
        {
            Instance = this;
        }


        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);

        }

        public override void Load()
        {
            AnotherPurpleBooster.Hooks.Hook();

            HoldableRefill.Load();
            LongDashRefill.Load();
            ForceyHoldables.Load();
        }

        public override void Unload() 
        {
            AnotherPurpleBooster.Hooks.Unhook();

            HoldableRefill.Unload();
            LongDashRefill.Unload();
            ForceyHoldables.Unload();
        }

    }
}
