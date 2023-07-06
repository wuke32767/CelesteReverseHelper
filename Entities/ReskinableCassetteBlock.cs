//using Celeste.Mod.Entities;
//using Microsoft.Xna.Framework;
//using Monocle;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace Celeste.Mod.ReverseHelper.Entities
//{
//    [CustomEntity("ReverseHelper/ReskinableCassetteBlock")]
//    public class ReskinableCassetteBlock : CassetteBlock
//    {
//        private string texture_pressed { get; set; }
//        private string texture_solid { get; set; }
//        static Random random;
//        public ReskinableCassetteBlock(EntityData data, Vector2 offset, EntityID id)
//    : this(data.Position + offset, id, data.Width, data.Height, data.Int("index"), data.Float("tempo", 1f), data.Attr("PressedTexture", "objects/ReverseHelper/ReskinableCassetteBlock/pressed"), data.Attr("SolidTexture", "objects/ReverseHelper/ReskinableCassetteBlock/solid"))
//        {
//        }

//        public ReskinableCassetteBlock(Vector2 position, EntityID id, float width, float height, int index, float tempo, string Tpressed, string TSolid)
//            : base(position, id, width, height, index, tempo)
//        {
//            if (Tpressed != "")
//            { 
//                texture_pressed = Tpressed;
//            }
//            else
//            {
//                texture_pressed = "";//TODO
//            }
//            texture_solid = TSolid;
//        }

//        //public override void Awake(Scene scene)
//        //{
//        //    base.Awake(scene);
//        //    var alls = GetType().BaseType.GetProperty("all");
//        //    List<Image> that = (List<Image>)alls?.GetValue(this);
//        //    if (that == null)
//        //    {
//        //        return;
//        //    }
//        //    random = random ?? new Random();
//        //    var pr = GFX.Game.GetAtlasSubtextures(texture_pressed);
//        //    var prc = GFX.Game[texture_solid];
//        //    foreach (var item in that)
//        //    {
//        //        if (item.Texture.AtlasPath == "objects/cassetteblock/pressed")
//        //        {
//        //            item.Texture = pr[Index % pr.Count].GetSubtexture(item.Texture.ClipRect);
//        //        }
//        //        else if (item.Texture.AtlasPath == "objects/cassetteblock/solid")
//        //        {
//        //            item.Texture = prc.GetSubtexture(item.Texture.ClipRect);
//        //        }
//        //    }
//        //}
//        public new bool BlockedCheck()
//        {
//            TheoCrystal theoCrystal = base.CollideFirst<TheoCrystal>();
//            if (theoCrystal != null && !this.TryActorWiggleUp(theoCrystal))
//            {
//                theoCrystal.Die();
//                return false;
//            }
//            Player player = base.CollideFirst<Player>();
//            if(player != null && !this.TryActorWiggleUp(player))
//            {
//                player.Die(new Vector2( 0,0));
//            }
//            return false;
//        }
//        private bool TryActorWiggleUp(Monocle.Entity actor)
//        {

//            bool collidable = this.Collidable;
//            this.Collidable = true;
//            for (int i = 1; i <= 4; i++)
//            {
//                if (!actor.CollideCheck<Solid>(actor.Position - Vector2.UnitY * (float)i))
//                {
//                    actor.Position -= Vector2.UnitY * (float)i;
//                    this.Collidable = collidable;
//                    return true;
//                }
//            }
//            this.Collidable = collidable;
//            return false;
//        }
//    }
//}