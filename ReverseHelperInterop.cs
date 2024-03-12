using Celeste.Mod.ReverseHelper.Entities;
using Monocle;
using MonoMod.ModInterop;
using System;

namespace Celeste.Mod.ReverseHelper
{
    [ModExportName("ReverseHelper.DreamBlock")]
    public static class Interop
    {
        /// <summary>
        /// if a entity acts like DreamBlock, use this.
        /// it should access Inventory.DreamDash only when awaking, and save it to a field.
        /// then, it controls visuals with that field and [De]ActivateNoRoutine. 
        /// and NO game logic relys on Inventory.DreamDash and that field.
        /// </summary>
        public static void RegisterDreamBlockLike(Type targetType, Action<Entity> ActivateNoRoutine, Action<Entity> DeactivateNoRoutine)
        {
            if (targetType is not null && ActivateNoRoutine is not null && DeactivateNoRoutine is not null)
            {
                ReversedDreamBlock.ExternalDreamBlockLike[targetType] = (ActivateNoRoutine, DeactivateNoRoutine);
            }
        }
        /// <summary>
        /// or, use this to check if your entity is enabled.
        /// notice that because of awake order, better to awake at the end of awake frame.
        /// </summary>
        /// <param name="e">the entity to be checked.</param>
        public static bool PlayerHasDreamDash(Entity e)
        {
            return ReversedDreamBlock.dreamblock_enabled(e);
        }
    }

}