using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.ReverseHelper.Libraries
{
    public class SingleGlobalEntity<For>() : Entity()
    {
        public static Action<Scene> OnSceneEnd = null!;
        public static void Register(Action<Scene> action)
        {
            OnSceneEnd = action;
        }
        public static void Construct(Scene scene)
        {
            if (self is not null || scene is null)
            {
                return;
            }
            self = new SingleGlobalEntity<For>();
            self.AddTag(Tags.Global);
            scene.Add(self);
            return;
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            OnSceneEnd?.Invoke(scene);
        }
        static SingleGlobalEntity<For>? self;
    }
}