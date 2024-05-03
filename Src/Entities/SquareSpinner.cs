using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
#nullable disable
namespace Celeste.Mod.ReverseHelper.Entities
{
    [CustomEntity("ReverseHelper/SquareSpinner")]

    // Token: 0x0200019B RID: 411
    [Tracked(false)]
    public class SquareSpinner : Entity
    {
        // Token: 0x04000974 RID: 2420
        private string fgTextureLookup;

        // Token: 0x04000975 RID: 2421
        private string bgTextureLookup;

        private bool coreMode;
        private bool rainbow;
        Color coreredcolor;
        Color corebluecolor;
        // Token: 0x06000E3B RID: 3643 RVA: 0x0003342C File Offset: 0x0003162C
        public SquareSpinner(Vector2 position, bool attachToSolid, Color color, bool coreMode, bool rainbow,
            string fg,string bg, Color corecolor) : base(position)
        {
            fgTextureLookup = fg;
            bgTextureLookup = bg;
            this.color = color;
            corebluecolor = color;
            Tag = Tags.TransitionUpdate;
            Collider =
                //new Circle(6f, 0f, 0f),
                new Hitbox(12f, 12f, -6f, -6f);
            ;
            Visible = false;
            Add(new PlayerCollider(OnPlayer, null, null));
            Add(new HoldableCollider(OnHoldable, null));
            Add(new LedgeBlocker(null));
            Depth = -8500;
            AttachToSolid = attachToSolid;
            if (attachToSolid)
            {
                Add(new StaticMover
                {
                    OnShake = OnShake,
                    SolidChecker = IsRiding,
                    OnDestroy = RemoveSelf
                });
            }
            randomSeed = Calc.Random.Next();
            this.coreMode = coreMode;
            this.rainbow = rainbow;
            coreredcolor = corecolor;
        }

        // Token: 0x06000E3C RID: 3644 RVA: 0x00033557 File Offset: 0x00031757
        public SquareSpinner(EntityData data, Vector2 offset/*, CrystalColor color*/)
            : this(data.Position + offset, data.Bool("attachToSolid", false), /*color*/data.HexColor("color"), data.Bool("coreMode", false), data.Bool("rainbow", false),
                  data.Attr("fgTexture", "objects/ReverseHelper/SquareSpinner/fg_neon"),data.Attr("bgTexture", "objects/ReverseHelper/SquareSpinner/bg_neon"),
                  data.HexColor("coreColor"))
        {
        }

        // Token: 0x06000E3D RID: 3645 RVA: 0x00033578 File Offset: 0x00031778
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (coreMode)
            {
                Add(new CoreModeListener(this));
                if ((scene as Level).CoreMode == Session.CoreModes.Cold)
                {
                    color = corebluecolor;
                }
                else
                {
                    color = coreredcolor;
                }
            }
            if (InView())
            {
                CreateSprites();
            }
        }

        // Token: 0x06000E3E RID: 3646 RVA: 0x000335DD File Offset: 0x000317DD
        public void ForceInstantiate()
        {
            CreateSprites();
            Visible = true;
        }

        // Token: 0x06000E3F RID: 3647 RVA: 0x000335EC File Offset: 0x000317EC
        public override void Update()
        {
            if (!Visible)
            {
                Collidable = false;
                if (InView())
                {
                    Visible = true;
                    if (!expanded)
                    {
                        CreateSprites();
                    }
                    if (rainbow)
                    {
                        UpdateHue();
                    }
                }
            }
            else
            {
                base.Update();
                if (rainbow && Scene.OnInterval(0.08f, offset))
                {
                    UpdateHue();
                }
                if (Scene.OnInterval(0.25f, offset) && !InView())
                {
                    Visible = false;
                }
                if (Scene.OnInterval(0.05f, offset))
                {
                    Player entity = Scene.Tracker.GetEntity<Player>();
                    if (entity != null)
                    {
                        Collidable = Math.Abs(entity.X - X) < 128f && Math.Abs(entity.Y - Y) < 128f;
                    }
                }
            }
            if (filler != null)
            {
                filler.Position = Position;
            }
        }

        // Token: 0x06000E40 RID: 3648 RVA: 0x00033714 File Offset: 0x00031914
        private void UpdateHue()
        {
            foreach (Component component in Components)
            {
                Image image = component as Image;
                if (image != null)
                {
                    image.Color = GetHue(Position + image.Position);
                }
            }
            if (filler != null)
            {
                foreach (Component component2 in filler.Components)
                {
                    Image image2 = component2 as Image;
                    if (image2 != null)
                    {
                        image2.Color = GetHue(Position + image2.Position);
                    }
                }
            }
        }

        // Token: 0x06000E41 RID: 3649 RVA: 0x000337E8 File Offset: 0x000319E8
        private bool InView()
        {
            Camera camera = (Scene as Level).Camera;
            return X > camera.X - 16f && Y > camera.Y - 16f && X < camera.X + 320f + 16f && Y < camera.Y + 180f + 16f;
        }

        // Token: 0x06000E42 RID: 3650 RVA: 0x00033864 File Offset: 0x00031A64
        private void CreateSprites()
        {
            if (!expanded)
            {
                Calc.PushRandom(randomSeed);
                List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(fgTextureLookup);
                MTexture mtexture = Calc.Random.Choose(atlasSubtextures);

                //Color color = Color.White;
                if (rainbow)
                {
                    color = GetHue(Position);
                }
                if (!SolidCheck(new Vector2(X - 4f, Y - 4f)))
                {
                    Add(new Image(mtexture.GetSubtexture(0, 0, 14, 14, null)).SetOrigin(12f, 12f).SetColor(color));
                }
                if (!SolidCheck(new Vector2(X + 4f, Y - 4f)))
                {
                    Add(new Image(mtexture.GetSubtexture(10, 0, 14, 14, null)).SetOrigin(2f, 12f).SetColor(color));
                }
                if (!SolidCheck(new Vector2(X + 4f, Y + 4f)))
                {
                    Add(new Image(mtexture.GetSubtexture(10, 10, 14, 14, null)).SetOrigin(2f, 2f).SetColor(color));
                }
                if (!SolidCheck(new Vector2(X - 4f, Y + 4f)))
                {
                    Add(new Image(mtexture.GetSubtexture(0, 10, 14, 14, null)).SetOrigin(12f, 2f).SetColor(color));
                }
                foreach (Entity entity in Scene.Tracker.GetEntities<SquareSpinner>())
                {
                    SquareSpinner crystalStaticSpinner = (SquareSpinner)entity;
                    if (crystalStaticSpinner != this && crystalStaticSpinner.AttachToSolid == AttachToSolid && crystalStaticSpinner.X >= X && (crystalStaticSpinner.Position - Position).Length() < 24f)
                    {
                        AddSprite((Position + crystalStaticSpinner.Position) / 2f - Position);
                    }
                }
                Scene.Add(border = new Border(this, filler));
                expanded = true;
                Calc.PopRandom();
            }
        }

        // Token: 0x06000E43 RID: 3651 RVA: 0x00033B0C File Offset: 0x00031D0C
        private void AddSprite(Vector2 offset)
        {
            if (filler == null)
            {
                Scene.Add(filler = new Entity(Position));
                filler.Depth = Depth + 1;
            }
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(bgTextureLookup);
            Image image = new Image(Calc.Random.Choose(atlasSubtextures));
            image.Position = offset;
            image.Rotation = (float)Calc.Random.Choose(0, 1, 2, 3) * 1.57079637f;
            image.CenterOrigin();
            image.SetColor(color);
            if (rainbow)
            {
                image.Color = GetHue(Position + offset);
            }
            filler.Add(image);
        }

        // Token: 0x06000E44 RID: 3652 RVA: 0x00033BD8 File Offset: 0x00031DD8
        private bool SolidCheck(Vector2 position)
        {
            if (AttachToSolid)
            {
                return false;
            }
            using (List<Solid>.Enumerator enumerator = Scene.CollideAll<Solid>(position).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current is SolidTiles)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // Token: 0x06000E45 RID: 3653 RVA: 0x00033C44 File Offset: 0x00031E44
        private void ClearSprites()
        {
            if (filler != null)
            {
                filler.RemoveSelf();
            }
            filler = null;
            if (border != null)
            {
                border.RemoveSelf();
            }
            border = null;
            foreach (Image image in Components.GetAll<Image>())
            {
                image.RemoveSelf();
            }
            expanded = false;
        }

        // Token: 0x06000E46 RID: 3654 RVA: 0x00033CD0 File Offset: 0x00031ED0
        private void OnShake(Vector2 pos)
        {
            foreach (Component component in Components)
            {
                if (component is Image)
                {
                    (component as Image).Position = pos;
                }
            }
        }

        // Token: 0x06000E47 RID: 3655 RVA: 0x00033D2C File Offset: 0x00031F2C
        private bool IsRiding(Solid solid)
        {
            return CollideCheck(solid);
        }

        // Token: 0x06000E48 RID: 3656 RVA: 0x00033D35 File Offset: 0x00031F35
        private void OnPlayer(Player player)
        {
            player.Die((player.Position - Position).SafeNormalize(), false, true);
        }

        // Token: 0x06000E49 RID: 3657 RVA: 0x00033D56 File Offset: 0x00031F56
        private void OnHoldable(Holdable h)
        {
            h.HitSpinner(this);
        }

        // Token: 0x06000E4A RID: 3658 RVA: 0x00033D60 File Offset: 0x00031F60
        public override void Removed(Scene scene)
        {
            if (filler != null && filler.Scene == scene)
            {
                filler.RemoveSelf();
            }
            if (border != null && border.Scene == scene)
            {
                border.RemoveSelf();
            }
            base.Removed(scene);
        }

        // Token: 0x06000E4B RID: 3659 RVA: 0x00033DB8 File Offset: 0x00031FB8
        public void Destroy(bool boss = false)
        {
            if (InView())
            {
                Audio.Play("event:/game/06_reflection/fall_spike_smash", Position);
                CrystalDebris.Burst(Position, color, boss, 8);
            }
            RemoveSelf();
        }

        // Token: 0x06000E4C RID: 3660 RVA: 0x00033E38 File Offset: 0x00032038
        private Color GetHue(Vector2 position)
        {
            float num = 280f;
            float value = (position.Length() + Scene.TimeActive * 50f) % num / num;
            return Calc.HsvToColor(0.4f + Calc.YoYo(value) * 0.4f, 0.4f, 0.9f);
        }

        // Token: 0x04000972 RID: 2418
        public static ParticleType P_Move { get => CrystalStaticSpinner.P_Move; }

        // Token: 0x04000973 RID: 2419
        public const float ParticleInterval = 0.02f;

        // Token: 0x04000976 RID: 2422
        public bool AttachToSolid;

        // Token: 0x04000977 RID: 2423
        private Entity filler;

        // Token: 0x04000978 RID: 2424
        private Border border;

        // Token: 0x04000979 RID: 2425
        private float offset = Calc.Random.NextFloat();

        // Token: 0x0400097A RID: 2426
        private bool expanded;

        // Token: 0x0400097B RID: 2427
        private int randomSeed;

        // Token: 0x0400097C RID: 2428
        private Color color;

        // Token: 0x02000499 RID: 1177
        private class CoreModeListener : Component
        {
            // Token: 0x0600233F RID: 9023 RVA: 0x000EE26C File Offset: 0x000EC46C
            public CoreModeListener(SquareSpinner parent) : base(true, false)
            {
                Parent = parent;
            }

            // Token: 0x06002340 RID: 9024 RVA: 0x000EE280 File Offset: 0x000EC480
            public override void Update()
            {
                Level level = Scene as Level;
                if ((Parent.color == Parent.corebluecolor && level.CoreMode == Session.CoreModes.Hot) || (Parent.color == Parent.coreredcolor && level.CoreMode == Session.CoreModes.Cold))
                {
                    if (Parent.color == Parent.corebluecolor)
                    {
                        Parent.color = Parent.coreredcolor;
                    }
                    else
                    {
                        Parent.color = Parent.corebluecolor;
                    }
                    Parent.ClearSprites();
                    Parent.CreateSprites();
                }
            }

            // Token: 0x040022D3 RID: 8915
            public SquareSpinner Parent;
        }

        // Token: 0x0200049A RID: 1178
        private class Border : Entity
        {
            // Token: 0x06002341 RID: 9025 RVA: 0x000EE303 File Offset: 0x000EC503
            public Border(Entity parent, Entity filler)
            {
                drawing[0] = parent;
                drawing[1] = filler;
                Depth = parent.Depth + 2;
            }

            // Token: 0x06002342 RID: 9026 RVA: 0x000EE337 File Offset: 0x000EC537
            public override void Render()
            {
                if (!drawing[0].Visible)
                {
                    return;
                }
                DrawBorder(drawing[0]);
                DrawBorder(drawing[1]);
            }

            // Token: 0x06002343 RID: 9027 RVA: 0x000EE368 File Offset: 0x000EC568
            private void DrawBorder(Entity entity)
            {
                if (entity == null)
                {
                    return;
                }
                foreach (Component component in entity.Components)
                {
                    Image image = component as Image;
                    if (image != null)
                    {
                        Color color = image.Color;
                        Vector2 position = image.Position;
                        image.Color = Color.Black;
                        image.Position = position + new Vector2(0f, -1f);
                        image.Render();
                        image.Position = position + new Vector2(0f, 1f);
                        image.Render();
                        image.Position = position + new Vector2(-1f, 0f);
                        image.Render();
                        image.Position = position + new Vector2(1f, 0f);
                        image.Render();
                        image.Color = color;
                        image.Position = position;
                    }
                }
            }

            // Token: 0x040022D4 RID: 8916
            private Entity[] drawing = new Entity[2];
        }
    }
}
#nullable restore