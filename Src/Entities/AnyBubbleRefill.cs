//using Celeste.Mod.Entities;
//using Microsoft.Xna.Framework;
//using Monocle;
//using System;
//using System.Collections;
//using System.Reflection;

//namespace Celeste.Mod.ReverseHelper.Entities
//{ 
//    internal class BubbleOneUseComponent:Component
//    {
//        BubbleOneUseComponent():base(true,false) { }
//    }
//    [CustomEntity("ReverseHelper/AnyBubbleRefill")]
//    public class AnyBubbleRefill : Entity
//    {
//        private int ID;

//        public static void Load()
//        {
//            On.Celeste.Booster.BoostRoutine += BoosterDeath;
//        }

//        private static FieldInfo boosterresp = typeof(Booster).GetField("respawnTimer", BindingFlags.NonPublic);

//        private static IEnumerator BoosterDeath(On.Celeste.Booster.orig_BoostRoutine orig, Booster self, Player player, Vector2 direction)
//        {
//            var flag = self.Get<BubbleOneUseComponent>();
//            if (!(flag is null))
//            {
//                boosterresp.SetValue(self, 99999f);
//            }
//            yield return new SwapImmediately(orig(self, player, direction));
//            if (!(flag is null))
//            {
//                yield return 0.8f;
//                self.RemoveSelf();
//            }
//            yield break;
//        }

//        public static void Unload()
//        {
//            On.Celeste.Booster.BoostRoutine -= BoosterDeath;
//        }

//        // Token: 0x02000350 RID: 848
//        // Token: 0x06001A97 RID: 6807 RVA: 0x000ABCD8 File Offset: 0x000A9ED8
//        public AnyBubbleRefill(Vector2 position, bool oneUse, int ID) : base(position)
//        {
//            this.ID = ID;
//            Collider = new Hitbox(16f, 16f, -8f, -8f);
//            Add(new PlayerCollider(new Action<Player>(OnPlayer), null, null));
//            twoDashes = false;
//            this.oneUse = oneUse;
//            string str;
//            if (twoDashes)
//            {
//                str = "objects/refillTwo/";
//                p_shatter = Refill.P_ShatterTwo;
//                p_regen = Refill.P_RegenTwo;
//                p_glow = Refill.P_GlowTwo;
//            }
//            else
//            {
//                str = "objects/refill/";
//                p_shatter = Refill.P_Shatter;
//                p_regen = Refill.P_Regen;
//                p_glow = Refill.P_Glow;
//            }
//            Add(outline = new Image(GFX.Game[str + "outline"]));
//            outline.CenterOrigin();
//            outline.Visible = false;
//            Add(sprite = new Sprite(GFX.Game, str + "idle"));
//            sprite.AddLoop("idle", "", 0.1f);
//            sprite.Play("idle", false, false);
//            sprite.CenterOrigin();
//            Add(flash = new Sprite(GFX.Game, str + "flash"));
//            flash.Add("flash", "", 0.05f);
//            flash.OnFinish = delegate (string anim)
//            {
//                flash.Visible = false;
//            };
//            flash.CenterOrigin();
//            Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v)
//            {
//                sprite.Scale = (flash.Scale = Vector2.One * (1f + v * 0.2f));
//            }, false, false));
//            Add(new MirrorReflection());
//            Add(bloom = new BloomPoint(0.8f, 16f));
//            Add(light = new VertexLight(Color.White, 1f, 16, 48));
//            Add(sine = new SineWave(0.6f, 0f));
//            sine.Randomize();
//            UpdateY();
//            Depth = -100;
//        }

//        // Token: 0x06001A98 RID: 6808 RVA: 0x000ABF38 File Offset: 0x000AA138
//        public AnyBubbleRefill(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("oneUse", false), data.ID)
//        {
//        }

//        // Token: 0x06001A99 RID: 6809 RVA: 0x000ABF64 File Offset: 0x000AA164
//        public override void Added(Scene scene)
//        {
//            base.Added(scene);
//            level = SceneAs<Level>();
//        }

//        // Token: 0x06001A9A RID: 6810 RVA: 0x000ABF7C File Offset: 0x000AA17C
//        public override void Update()
//        {
//            base.Update();
//            if (respawnTimer > 0f)
//            {
//                respawnTimer -= Engine.DeltaTime;
//                if (respawnTimer <= 0f)
//                {
//                    Respawn();
//                }
//            }
//            else if (Scene.OnInterval(0.1f))
//            {
//                level.ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
//            }
//            UpdateY();
//            light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
//            bloom.Alpha = light.Alpha * 0.8f;
//            if (Scene.OnInterval(2f) && sprite.Visible)
//            {
//                flash.Play("flash", true, false);
//                flash.Visible = true;
//            }
//        }

//        // Token: 0x06001A9B RID: 6811 RVA: 0x000AC0A0 File Offset: 0x000AA2A0
//        private void Respawn()
//        {
//            if (!Collidable)
//            {
//                Collidable = true;
//                sprite.Visible = true;
//                outline.Visible = false;
//                Depth = -100;
//                wiggler.Start();
//                Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_return" : "event:/game/general/diamond_return", Position);
//                level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
//            }
//        }

//        // Token: 0x06001A9C RID: 6812 RVA: 0x000AC134 File Offset: 0x000AA334
//        private void UpdateY()
//        {
//            flash.Y = (sprite.Y = (bloom.Y = sine.Value * 2f));
//        }

//        // Token: 0x06001A9D RID: 6813 RVA: 0x000AC179 File Offset: 0x000AA379
//        public override void Render()
//        {
//            if (sprite.Visible)
//            {
//                sprite.DrawOutline(1);
//            }
//            base.Render();
//        }

//        // Token: 0x06001A9E RID: 6814 RVA: 0x000AC19C File Offset: 0x000AA39C
//        private void OnPlayer(Player player)
//        {
//            if (player.UseRefill(twoDashes))
//            {
//                Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", Position);
//                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
//                Collidable = false;
//                Add(new Coroutine(RefillRoutine(player), true));
//                respawnTimer = 2.5f;
//            }
//        }

//        // Token: 0x06001A9F RID: 6815 RVA: 0x000AC203 File Offset: 0x000AA403
//        private IEnumerator RefillRoutine(Player player)
//        {
//            Celeste.Freeze(0.05f);
//            yield return null;
//            level.Shake(0.3f);
//            sprite.Visible = (flash.Visible = false);
//            if (!oneUse)
//            {
//                outline.Visible = true;
//            }
//            Depth = 8999;
//            yield return 0.05f;
//            float num = player.Speed.Angle();
//            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - 1.57079637f);
//            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + 1.57079637f);
//            SlashFx.Burst(Position, num);
//            if (oneUse)
//            {
//                RemoveSelf();
//            }
//            yield break;
//        }

//        // Token: 0x04001731 RID: 5937
//        public static ParticleType P_Shatter { get => Refill.P_Shatter; }

//        // Token: 0x04001732 RID: 5938
//        public static ParticleType P_Regen { get => Refill.P_Regen; }

//        // Token: 0x04001733 RID: 5939
//        public static ParticleType P_Glow { get => Refill.P_Glow; }

//        // Token: 0x04001734 RID: 5940
//        public static ParticleType P_ShatterTwo { get => Refill.P_ShatterTwo; }

//        // Token: 0x04001735 RID: 5941
//        public static ParticleType P_RegenTwo { get => Refill.P_RegenTwo; }

//        // Token: 0x04001736 RID: 5942
//        public static ParticleType P_GlowTwo { get => Refill.P_GlowTwo; }

//        // Token: 0x04001737 RID: 5943
//        private Sprite sprite;

//        // Token: 0x04001738 RID: 5944
//        private Sprite flash;

//        // Token: 0x04001739 RID: 5945
//        private Image outline;

//        // Token: 0x0400173A RID: 5946
//        private Wiggler wiggler;

//        // Token: 0x0400173B RID: 5947
//        private BloomPoint bloom;

//        // Token: 0x0400173C RID: 5948
//        private VertexLight light;

//        // Token: 0x0400173D RID: 5949
//        private Level level;

//        // Token: 0x0400173E RID: 5950
//        private SineWave sine;

//        // Token: 0x0400173F RID: 5951
//        private bool twoDashes;

//        // Token: 0x04001740 RID: 5952
//        private bool oneUse;

//        // Token: 0x04001741 RID: 5953
//        private ParticleType p_shatter;

//        // Token: 0x04001742 RID: 5954
//        private ParticleType p_regen;

//        // Token: 0x04001743 RID: 5955
//        private ParticleType p_glow;

//        // Token: 0x04001744 RID: 5956
//        private float respawnTimer;
//    }
//}