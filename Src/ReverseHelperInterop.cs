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
        /// <summary>
        /// If v is not null, set reverse mode to v.
        /// If v is null, get reverse mode.
        /// </summary>
        /// <returns><code>v ?? current</code> If reverse mode is not set, current is null. </returns>
        public static bool? ConfigureReverse(Entity e, bool? v)
        {
            if (v.HasValue)
            {
                DreamBlockConfig.Get(e).Reverse(v);
                return v;
            }
            else
            {
                DreamBlockConfig.TryGet(e, out var dc);
                return dc?.reverse;
            }
        }
        /// <summary>
        /// If v is not null, set enable mode to v.
        /// If v is null, get enable mode.
        /// </summary>
        /// <returns><code>v ?? current</code> If enable mode is not set, current is null. </returns>
        public static bool? ConfigureEnable(Entity e, bool? v)
        {
            if (v.HasValue)
            {
                DreamBlockConfig.Get(e).Enable(v);
                return v;
            }
            else
            {
                DreamBlockConfig.TryGet(e, out var dc);
                return dc?.enable;
            }
        }
        /// <summary>
        /// If v is not null, set disable mode to v.
        /// If v is null, get disable mode.
        /// </summary>
        /// <returns><code>v ?? current</code> If disable mode is not set, current is null. </returns>
        public static bool? ConfigureDisable(Entity e, bool? v)
        {
            if (v.HasValue)
            {
                DreamBlockConfig.Get(e).Disable(v);
                return v;
            }
            else
            {
                DreamBlockConfig.TryGet(e, out var dc);
                return dc?.disable;
            }
        }
        /// <summary>
        /// If v is not null, set high priority mode to v.
        /// If v is null, get high priority mode.
        /// It is not Platform.SurfaceSoundPriority.
        /// </summary>
        /// <returns><code>v ?? current</code> If high priority mode is not set, current is null. </returns>
        public static bool? ConfigureHighPriority(Entity e, bool? v)
        {
            if (v.HasValue)
            {
                DreamBlockConfig.Get(e).HighPriority(v);
                return v;
            }
            else
            {
                DreamBlockConfig.TryGet(e, out var dc);
                return dc?.highpriority;
            }
        }
    }

}