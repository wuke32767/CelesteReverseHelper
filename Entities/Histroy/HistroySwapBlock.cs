namespace Celeste.Mod.ReverseHelper.Entities.Histroy
{
    using FMOD.Studio;
    using global::Celeste.Mod.Entities;
    using Microsoft.Xna.Framework;
    using Monocle;
    using System;


    // Token: 0x020002D9 RID: 729
    [CustomEntity("ReverseHelper/RecordedSwapBlock")]
    public class HistroySwapBlock : Solid
    {
        public new void MoveTo(Vector2 position, Vector2 liftSpeed)
        {
            if(Engine.DeltaTime>0)
            {
                base.MoveToX(position.X, liftSpeed.X);
                base.MoveToY(position.Y, liftSpeed.Y);
            }
            else
            {
                base.MoveToX(position.X, -liftSpeed.X);
                base.MoveToY(position.Y, -liftSpeed.Y);
            }
        }

        // Token: 0x06001687 RID: 5767 RVA: 0x00084FAC File Offset: 0x000831AC
        public HistroySwapBlock(Vector2 position, float width, float height, Vector2 node, Themes theme) : base(position, width, height, false)
        {
            Theme = theme;
            start = Position;
            end = node;
            maxForwardSpeed = 360f / Vector2.Distance(start, end);
            maxBackwardSpeed = maxForwardSpeed * 0.4f;
            Direction.X = Math.Sign(end.X - start.X);
            Direction.Y = Math.Sign(end.Y - start.Y);
            Add(new DashListener
            {
                OnDash = new Action<Vector2>(OnDash)
            });
            int num = (int)MathHelper.Min(X, node.X);
            int num2 = (int)MathHelper.Min(Y, node.Y);
            int num3 = (int)MathHelper.Max(X + Width, node.X + Width);
            int num4 = (int)MathHelper.Max(Y + Height, node.Y + Height);
            moveRect = new Rectangle(num, num2, num3 - num, num4 - num2);
            MTexture mtexture;
            MTexture mtexture2;
            MTexture mtexture3;
            if (Theme == Themes.Moon)
            {
                mtexture = GFX.Game["objects/swapblock/moon/block"];
                mtexture2 = GFX.Game["objects/swapblock/moon/blockRed"];
                mtexture3 = GFX.Game["objects/swapblock/moon/target"];
            }
            else
            {
                mtexture = GFX.Game["objects/swapblock/block"];
                mtexture2 = GFX.Game["objects/swapblock/blockRed"];
                mtexture3 = GFX.Game["objects/swapblock/target"];
            }
            nineSliceGreen = new MTexture[3, 3];
            nineSliceRed = new MTexture[3, 3];
            nineSliceTarget = new MTexture[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    nineSliceGreen[i, j] = mtexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                    nineSliceRed[i, j] = mtexture2.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                    nineSliceTarget[i, j] = mtexture3.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                }
            }
            if (Theme == Themes.Normal)
            {
                Add(middleGreen = GFX.SpriteBank.Create("swapBlockLight"));
                Add(middleRed = GFX.SpriteBank.Create("swapBlockLightRed"));
            }
            else if (Theme == Themes.Moon)
            {
                Add(middleGreen = GFX.SpriteBank.Create("swapBlockLightMoon"));
                Add(middleRed = GFX.SpriteBank.Create("swapBlockLightRedMoon"));
            }
            Add(new LightOcclude(0.2f));
            Depth = -9999;

            hsm.SetCallbacks(stStatic,
            onUpdate: () =>
            {
                if(start!=Position)
                {
                    float num = lerp;
                    lerp = Calc.Approach(lerp, target, speed * Engine.DeltaTime);
                    if (lerp != num)
                    {
                        Vector2 position = Position;
                        MoveTo(Vector2.Lerp(start, end, lerp), (end - start) * -maxBackwardSpeed);
                        //if (position != Position)
                        {
                            Audio.Position(moveSfx, Center);
                            Audio.Position(returnSfx, Center);
                            if (Position == start && target == 0)
                            {
                                Audio.SetParameter(returnSfx, "end", 1f);
                                Audio.Play("event:/game/05_mirror_temple/swapblock_return_end", Center);
                            }
                        }
                    }
                }
                return stStatic;
            }, null, null, null,
            begin: () =>
            {
                target = 0;
                if (returnTimer > 0.0001f)
                {
                    //Logger.Log(LogLevel.Warn,"","")
                }
            }, null, null,
            rend: () =>
            {
                target = 0;
            });

            //hsm.SetCallbacks(stStaticAcc,
            //onUpdate: ()=> 
            //{
            //    speed = Calc.Approach(speed, maxBackwardSpeed, maxBackwardSpeed / 1.5f * Engine.DeltaTime);
            //    float num = lerp;
            //    lerp = Calc.Approach(lerp, target, speed * Engine.DeltaTime);
            //    if (lerp != num)
            //    {
            //        Vector2 vector = (end - start) * speed;
            //        Vector2 position = Position;
            //        if (lerp < num)
            //        {
            //            vector *= -1f;
            //        }
            //        MoveTo(Vector2.Lerp(start, end, lerp), vector);
            //        if (position != Position)
            //        {
            //            Audio.Position(moveSfx, Center);
            //            Audio.Position(returnSfx, Center);
            //            if (Position == start && target == 0)
            //            {
            //                Audio.SetParameter(returnSfx, "end", 1f);
            //                Audio.Play("event:/game/05_mirror_temple/swapblock_return_end", Center);
            //            }
            //            else if (Position == end && target == 1)
            //            {
            //                Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", Center);
            //            }
            //        }
            //    }
            //    if(lerp==0)
            //    {
            //        return stStatic;
            //    }
            //    if(speed==maxBackwardSpeed)
            //    {
            //        return stStatic;
            //    }
            //    return stStaticAcc;
            //},
            //ronUpdate: () =>
            //{

            //},
            //null, null,
            //begin: () =>
            //{
            //    target = 0;
            //    speed = 0;
            //},
            //rbegin: null,
            //end: () =>
            //{
            //    hsm.PushMessage(speed);
            //},
            //rend: () =>
            //{
            //    speed = hsm.PopMessage();
            //});
            void rswapacin()
            {
                target = 1;
                returnTimer = 0;

            }
            void swapacin()
            {
                target = 1;
                returnTimer = ReturnTime;

            }
            void swapout()
            {
                //speed = 0;
                returnSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_return", Center);
            }
            hsm.SetCallbacks(stSwap,
            onUpdate: () =>
            {
                returnTimer -= Engine.DeltaTime;
                return stSwap;
            },
            ronUpdate: () =>
            {
                returnTimer -= Engine.DeltaTime;
                return stSwap;
            }, null, null,
            begin: swapacin,
            rbegin: swapout,
            end: swapout,
            rend: rswapacin);
            hsm.SetCallbacks(stNotMoving,()=>stNotMoving);
            //hsm.SetCallbacks(stRNotMoving, () =>
            //{

            //});
        }
        public const int stStatic = 0;
        public const int stSwap = 1;
        public const int stStaticAcc = 2;
        public const int stSwapAcc = 3;
        public const int stNotMoving = 4;
        public const int stRNotMoving = 5;
        HistroyStateMachine<float> hsm = new(2);
        // Token: 0x06001688 RID: 5768 RVA: 0x000852E4 File Offset: 0x000834E4
        public HistroySwapBlock(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, data.Enum("theme", Themes.Normal))
        {
        }

        // Token: 0x06001689 RID: 5769 RVA: 0x00085324 File Offset: 0x00083524
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            scene.Add(path = new PathRenderer(this));
        }

        // Token: 0x0600168A RID: 5770 RVA: 0x0008534D File Offset: 0x0008354D
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Audio.Stop(moveSfx, true);
            Audio.Stop(returnSfx, true);
        }

        // Token: 0x0600168B RID: 5771 RVA: 0x0008536E File Offset: 0x0008356E
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Audio.Stop(moveSfx, true);
            Audio.Stop(returnSfx, true);
        }

        // Token: 0x0600168C RID: 5772 RVA: 0x00085390 File Offset: 0x00083590

        private void OnDash(Vector2 direction)
        {
            hsm.ExternalState(stSwapAcc);
            Swapping = lerp < 1f;
            target = 1;
            returnTimer = ReturnTime;
            burst = (Scene as Level)!.Displacement.AddBurst(Center, 0.2f, 0f, 16f, 1f, null, null);
            if (lerp >= 0.2f)
            {
                speed = maxForwardSpeed;
            }
            else
            {
                speed = MathHelper.Lerp(maxForwardSpeed * 0.333f, maxForwardSpeed, lerp / 0.2f);
            }
            Audio.Stop(returnSfx, true);
            Audio.Stop(moveSfx, true);
            if (!Swapping)
            {
                Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", Center);
                return;
            }
            moveSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_move", Center);
        }

        // Token: 0x0600168D RID: 5773 RVA: 0x00085488 File Offset: 0x00083688
        //Can we cut it by frames?
        public override void Update()
        {
            base.Update();
            //if (returnTimer > 0f)
            //{
            //    returnTimer -= Engine.DeltaTime;
            //    if (returnTimer <= 0f)
            //    {
            //        target = 0;
            //        speed = 0f;
            //        returnSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_return", Center);
            //    }
            //}
            if (burst != null)
            {
                burst.Position = Center;
            }
            redAlpha = Calc.Approach(redAlpha, (target == 1) ? 0 : 1, Math.Abs(Engine.DeltaTime * 32f));
            if (target == 0 && lerp == 0f)
            {
                middleRed.SetAnimationFrame(0);
                middleGreen.SetAnimationFrame(0);
            }
            if (target == 1)
            {
                speed = Calc.Approach(speed, maxForwardSpeed, maxForwardSpeed / 0.2f * Engine.DeltaTime);
            }
            else
            {
                //speed = Calc.Approach(speed, maxBackwardSpeed, maxBackwardSpeed / 1.5f * Engine.DeltaTime);
            }
            float num = lerp;
            lerp = Calc.Approach(lerp, target, speed * Engine.DeltaTime);
            if (lerp != num)
            {
                Vector2 vector = (end - start) * speed;
                Vector2 position = Position;
                if (target == 1)
                {
                    vector = (end - start) * maxForwardSpeed;
                }
                if (lerp < num)
                {
                    vector *= -1f;
                }
                if (target == 1 && Scene.OnInterval(0.02f))
                {
                    MoveParticles(end - start);
                }
                MoveTo(Vector2.Lerp(start, end, lerp), vector);
                if (position != Position)
                {
                    Audio.Position(moveSfx, Center);
                    Audio.Position(returnSfx, Center);
                    if (Position == start && target == 0)
                    {
                        Audio.SetParameter(returnSfx, "end", 1f);
                        Audio.Play("event:/game/05_mirror_temple/swapblock_return_end", Center);
                    }
                    else if (Position == end && target == 1)
                    {
                        Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", Center);
                    }
                }
            }
            if (Swapping && lerp >= 1f)
            {
                Swapping = false;
            }
            StopPlayerRunIntoAnimation = (lerp <= 0f || lerp >= 1f);
        }

        // Token: 0x0600168E RID: 5774 RVA: 0x00085788 File Offset: 0x00083988
        private void MoveParticles(Vector2 normal)
        {
            Vector2 position;
            Vector2 vector;
            float direction;
            float num;
            if (normal.X > 0f)
            {
                position = CenterLeft;
                vector = Vector2.UnitY * (Height - 6f);
                direction = 3.14159274f;
                num = Math.Max(2f, Height / 14f);
            }
            else if (normal.X < 0f)
            {
                position = CenterRight;
                vector = Vector2.UnitY * (Height - 6f);
                direction = 0f;
                num = Math.Max(2f, Height / 14f);
            }
            else if (normal.Y > 0f)
            {
                position = TopCenter;
                vector = Vector2.UnitX * (Width - 6f);
                direction = -1.57079637f;
                num = Math.Max(2f, Width / 14f);
            }
            else
            {
                position = BottomCenter;
                vector = Vector2.UnitX * (Width - 6f);
                direction = 1.57079637f;
                num = Math.Max(2f, Width / 14f);
            }
            particlesRemainder += num;
            int num2 = (int)particlesRemainder;
            particlesRemainder -= num2;
            vector *= 0.5f;
            SceneAs<Level>().Particles.Emit(P_Move, num2, position, vector, direction);
        }

        // Token: 0x0600168F RID: 5775 RVA: 0x00085904 File Offset: 0x00083B04
        public override void Render()
        {
            Vector2 vector = Position + Shake;
            if (lerp != target && speed > 0f)
            {
                Vector2 value = (end - start).SafeNormalize();
                if (target == 1)
                {
                    value *= -1f;
                }
                float num = speed / maxForwardSpeed;
                float num2 = 16f * num;
                int num3 = 2;
                while (num3 < num2)
                {
                    DrawBlockStyle(vector + value * num3, Width, Height, nineSliceGreen, middleGreen, Color.White * (1f - num3 / num2));
                    num3 += 2;
                }
            }
            if (redAlpha < 1f)
            {
                DrawBlockStyle(vector, Width, Height, nineSliceGreen, middleGreen, Color.White);
            }
            if (redAlpha > 0f)
            {
                DrawBlockStyle(vector, Width, Height, nineSliceRed, middleRed, Color.White * redAlpha);
            }
        }

        // Token: 0x06001690 RID: 5776 RVA: 0x00085A48 File Offset: 0x00083C48
        private void DrawBlockStyle(Vector2 pos, float width, float height, MTexture[,] ninSlice, Sprite middle, Color color)
        {
            int num = (int)(width / 8f);
            int num2 = (int)(height / 8f);
            ninSlice[0, 0].Draw(pos + new Vector2(0f, 0f), Vector2.Zero, color);
            ninSlice[2, 0].Draw(pos + new Vector2(width - 8f, 0f), Vector2.Zero, color);
            ninSlice[0, 2].Draw(pos + new Vector2(0f, height - 8f), Vector2.Zero, color);
            ninSlice[2, 2].Draw(pos + new Vector2(width - 8f, height - 8f), Vector2.Zero, color);
            for (int i = 1; i < num - 1; i++)
            {
                ninSlice[1, 0].Draw(pos + new Vector2(i * 8, 0f), Vector2.Zero, color);
                ninSlice[1, 2].Draw(pos + new Vector2(i * 8, height - 8f), Vector2.Zero, color);
            }
            for (int j = 1; j < num2 - 1; j++)
            {
                ninSlice[0, 1].Draw(pos + new Vector2(0f, j * 8), Vector2.Zero, color);
                ninSlice[2, 1].Draw(pos + new Vector2(width - 8f, j * 8), Vector2.Zero, color);
            }
            for (int k = 1; k < num - 1; k++)
            {
                for (int l = 1; l < num2 - 1; l++)
                {
                    ninSlice[1, 1].Draw(pos + new Vector2(k, l) * 8f, Vector2.Zero, color);
                }
            }
            if (middle != null)
            {
                middle.Color = color;
                middle.RenderPosition = pos + new Vector2(width / 2f, height / 2f);
                middle.Render();
            }
        }

        // Token: 0x040012F6 RID: 4854
        public static ParticleType P_Move { get => SwapBlock.P_Move; }

        // Token: 0x040012F7 RID: 4855
        private const float ReturnTime = 0.8f;

        // Token: 0x040012F8 RID: 4856
        public Vector2 Direction;

        // Token: 0x040012F9 RID: 4857
        public bool Swapping;

        // Token: 0x040012FA RID: 4858
        public Themes Theme;

        // Token: 0x040012FB RID: 4859
        private Vector2 start;

        // Token: 0x040012FC RID: 4860
        private Vector2 end;

        // Token: 0x040012FD RID: 4861
        private float lerp;

        // Token: 0x040012FE RID: 4862
        private int target;

        // Token: 0x040012FF RID: 4863
        private Rectangle moveRect;

        // Token: 0x04001300 RID: 4864
        private float speed;

        // Token: 0x04001301 RID: 4865
        private float maxForwardSpeed;

        // Token: 0x04001302 RID: 4866
        private float maxBackwardSpeed;

        // Token: 0x04001303 RID: 4867
        private float returnTimer;

        // Token: 0x04001304 RID: 4868
        private float redAlpha = 1f;

        // Token: 0x04001305 RID: 4869
        private MTexture[,] nineSliceGreen;

        // Token: 0x04001306 RID: 4870
        private MTexture[,] nineSliceRed;

        // Token: 0x04001307 RID: 4871
        private MTexture[,] nineSliceTarget;

        // Token: 0x04001308 RID: 4872
        private Sprite middleGreen;

        // Token: 0x04001309 RID: 4873
        private Sprite middleRed;

        // Token: 0x0400130A RID: 4874
        private PathRenderer path;

        // Token: 0x0400130B RID: 4875
        private EventInstance moveSfx;

        // Token: 0x0400130C RID: 4876
        private EventInstance returnSfx;

        // Token: 0x0400130D RID: 4877
        private DisplacementRenderer.Burst burst;

        // Token: 0x0400130E RID: 4878
        private float particlesRemainder;

        // Token: 0x02000676 RID: 1654
        public enum Themes
        {
            // Token: 0x04002ACF RID: 10959
            Normal,

            // Token: 0x04002AD0 RID: 10960
            Moon
        }

        // Token: 0x02000677 RID: 1655
        private class PathRenderer : Entity
        {
            // Token: 0x06002BA4 RID: 11172 RVA: 0x00117C90 File Offset: 0x00115E90
            public PathRenderer(HistroySwapBlock block) : base(block.Position)
            {
                this.block = block;
                Depth = 8999;
                pathTexture = GFX.Game["objects/swapblock/path" + (block.start.X == block.end.X ? "V" : "H")];
                timer = Calc.Random.NextFloat();
            }

            // Token: 0x06002BA5 RID: 11173 RVA: 0x00117D14 File Offset: 0x00115F14
            public override void Update()
            {
                base.Update();
                timer += Engine.DeltaTime * 4f;
            }

            // Token: 0x06002BA6 RID: 11174 RVA: 0x00117D34 File Offset: 0x00115F34
            public override void Render()
            {
                if (block.Theme != Themes.Moon)
                {
                    for (int i = block.moveRect.Left; i < block.moveRect.Right; i += pathTexture.Width)
                    {
                        for (int j = block.moveRect.Top; j < block.moveRect.Bottom; j += pathTexture.Height)
                        {
                            pathTexture.GetSubtexture(0, 0, Math.Min(pathTexture.Width, block.moveRect.Right - i), Math.Min(pathTexture.Height, block.moveRect.Bottom - j), clipTexture);
                            clipTexture.DrawCentered(new Vector2(i + clipTexture.Width / 2, j + clipTexture.Height / 2), Color.White);
                        }
                    }
                }
                float scale = 0.5f * (0.5f + ((float)Math.Sin(timer) + 1f) * 0.25f);
                block.DrawBlockStyle(new Vector2(block.moveRect.X, block.moveRect.Y), block.moveRect.Width, block.moveRect.Height, block.nineSliceTarget, null, Color.White * scale);
            }

            // Token: 0x04002AD1 RID: 10961
            private HistroySwapBlock block;

            // Token: 0x04002AD2 RID: 10962
            private MTexture pathTexture;

            // Token: 0x04002AD3 RID: 10963
            private MTexture clipTexture = new MTexture();

            // Token: 0x04002AD4 RID: 10964
            private float timer;
        }
    }
}