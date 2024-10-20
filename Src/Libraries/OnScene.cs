using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.ReverseHelper.Libraries
{
    public class OnScene<For>() : Entity()
    {
        public static Action<Scene> OnSceneEnd = null!;
        public static Action<Scene> OnUpdate = null!;
        public static bool Construct(Scene scene)
        {
            if (scene is null || self.Scene == scene)
            {
                return false;
            }

            self.AddTag(Tags.Global);
            scene.Add(self);
            return true;
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            OnSceneEnd?.Invoke(scene);
            RemoveSelf();
            Scene = null;
        }
        public static readonly OnScene<For> self=new();
        public override void Update()
        {
            base.Update();
            OnUpdate?.Invoke(Scene);
        }
    }
}