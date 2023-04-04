using System;

namespace Celeste.Mod.ReverseHelper 
{
    public class ReverseHelperModule : EverestModule 
    {
        
        public static ReverseHelperModule Instance;

        public ReverseHelperModule() 
        {
            Instance = this;
        }

        public override void Load()
        {
        }

        public override void Unload() 
        {
        }

    }
}
