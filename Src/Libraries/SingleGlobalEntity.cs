using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.ReverseHelper.Libraries
{
    public class SingleGlobalEntity<For>() : Entity()
    {
        public static Action<Scene> OnSceneEnd = null!;
        public static int Register(Action<Scene>action)
        {
            OnSceneEnd = action;
            return 0;
        }
        public static int Construct(Scene scene)
        {
            if (self is not null || scene is null)
            {
                return 0;
            }
            var e = new SingleGlobalEntity<For>();
            e.AddTag(Tags.Global);
            scene.Add(e);
            return 0;
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            OnSceneEnd?.Invoke(scene);
        }
        static SingleGlobalEntity<For>? self;
    }
}