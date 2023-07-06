using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.ReverseHelper 
{
    public class ReverseHelperModule : EverestModule 
    {
        //class img
        //{
        //    //public byte[] data = new byte[1024*16*1024];
        //    //public int col, row;

        //}
        //bool debug = true;
        public static ReverseHelperModule Instance;
        public ReverseHelperModule() 
        {
            Instance = this;
            //Task.Run(() =>
            //{
            //    while (debug)
            //    {
            //        Thread.Sleep(1000);
            //        if(!debug)
            //        {
            //            img img=new img();
            //            string s="";
            //            var i=GFX.Game[s];
            //            var tx=i.Texture.Texture_Safe;
            //            tx.GetData(img.data);
            //            img.col=tx.Width;
            //            img.row=tx.Height;
            //        }
            //    }
            //});
        }

        public override void Load()
        {
            //Entities.HoldableRefill.Load();
        }

        public override void Unload() 
        {
            //Entities.HoldableRefill.Unload();
        }

    }
}
