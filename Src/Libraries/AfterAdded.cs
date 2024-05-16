using Monocle;
using System;

namespace Celeste.Mod.ReverseHelper.Libraries
{
    class AfterAdded : Entity
    {
        Action action = default!;
        static AfterAdded? Inst;
        int donotcollision;
        AfterAdded(int donotcollision = 3) : base()
        {
            this.donotcollision = donotcollision;
        }
        public static void Reg(Scene scene, Action act)
        {
            if (Inst is null)
            {
                Inst = new();
            }
            Inst.action += act;
            scene.Add(Inst);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (scene.Entities.ToAdd[^1] == this || donotcollision == 0)
            {
                action();
                Inst = null;
            }
            else//move to the end of toadd list
            {
                scene.Add(Inst = new(donotcollision - 1) { action = action });
            }
            RemoveSelf();
        }
    }
}