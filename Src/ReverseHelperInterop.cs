using Celeste.Mod.ReverseHelper.Entities;
using MonoMod.ModInterop;
using System.Collections.Immutable;

namespace Celeste.Mod.ReverseHelper
{
    public static class DreamBlockInteropImportCallerTemplate
    {
        /// <summary>
        /// if a entity acts like DreamBlock, use this. 
        /// it should access Inventory.DreamDash only when awaking, and save it to a field.
        /// then, it controls visuals with that field and [De]ActivateNoRoutine. 
        /// and NO game logic relys on Inventory.DreamDash.
        /// 
        /// anyway, better to ask me for it.
        /// </summary>
        public static void RegisterDreamBlockLike(Type targetType, Action<Entity> ActivateNoRoutine, Action<Entity> DeactivateNoRoutine)
        {
            DreamBlockInteropImportTemplate.RegisterDreamBlockLike?.Invoke(targetType, ActivateNoRoutine, DeactivateNoRoutine);
        }
        /// <summary> 
        /// or, use this to check if your entity is enabled.
        /// notice that because of awake order, your entity might awake before reversed.
        /// better to awake at the end of awake frame.
        /// </summary>
        /// <param name="e">the entity to be checked.</param>
        /// <param name="fallback">if ReverseHelper is not loaded, use fallback instead.</param>
        public static bool PlayerHasDreamDash(Entity e, Func<bool>? fallback = null)
        {
            if (DreamBlockInteropImportTemplate.PlayerHasDreamDash is null)
            {
                return fallback?.Invoke() ?? (Engine.Scene as Level)?.Session?.Inventory.DreamDash ?? false;
            }
            return DreamBlockInteropImportTemplate.PlayerHasDreamDash(e);
        }
        /// <summary>
        /// generic version of these options.
        /// https://github.com/wuke32767/CelesteReverseHelper/blob/main/Src/Entities/DreamBlockConfigurer.cs#L21
        /// those option name and value will not be changed (wip excluded), so you can just hardcode them.
        /// getter.
        /// </summary>
        /// <returns>
        /// if this flag is set.
        /// null: not set / not loaded.
        /// not null: get flag.
        /// </returns>
        public static bool? ConfigureGetFromEnum(Entity e, long i)
        {
            return DreamBlockInteropImportTemplate.ConfigureGetFromEnum?.Invoke(e, i);
        }
        /// <summary>
        /// generic version of these option.
        /// setter.
        /// </summary>
        /// notice that it can clear a flag.
        public static void ConfigureSetFromEnum(Entity e, long i, bool? value)
        {
            DreamBlockInteropImportTemplate.ConfigureSetFromEnum?.Invoke(e, i, value);
        }
        /// <summary>
        /// get option from string.
        /// </summary>
        /// <returns> one of Enum.GetValues<DreamBlockConfigFlags>() </returns>
        /// (param) one of Enum.GetNames<DreamBlockConfigFlags>()
        public static long ConfigureGetEnum(string s)
        {
            return DreamBlockInteropImportTemplate.ConfigureGetEnum?.Invoke(s) ?? 0;
        }
        /// <summary>
        /// returns trackers for these flags.
        /// </summary>
        /// <returns>
        /// index it with the enum and you would get all dreamblock that has the flag.
        /// sometimes it contains Dream Tunnel [Communal Helper], which is not DreamBlock. 
        /// </returns>
        public static ImmutableArray<List<Entity>>? GetDreamBlockTrackers(Scene scene)
        {
            return DreamBlockInteropImportTemplate.GetDreamBlockTrackers?.Invoke(scene);
        }
    }

    [ModImportName("ReverseHelper.DreamBlock")]
    public static class DreamBlockInteropImportTemplate
    {
        public static Action<Type, Action<Entity>, Action<Entity>>? RegisterDreamBlockLike;
        public static Func<Entity, bool>? PlayerHasDreamDash;
        public static Func<Entity, long, bool?>? ConfigureGetFromEnum;
        public static Action<Entity, long, bool?>? ConfigureSetFromEnum;
        public static Func<string, long>? ConfigureGetEnum;
        public static Func<Scene, ImmutableArray<List<Entity>>>? GetDreamBlockTrackers;
    }
    [ModExportName("ReverseHelper.DreamBlock")]
    public static class DreamBlockInterop
    {
        public static void RegisterDreamBlockLike(Type targetType, Action<Entity> ActivateNoRoutine, Action<Entity> DeactivateNoRoutine)
        {
            if (targetType is not null && ActivateNoRoutine is not null && DeactivateNoRoutine is not null)
            {
                DreamBlockConfigurer.ExternalDreamBlockLike[targetType] = (ActivateNoRoutine, DeactivateNoRoutine);
            }
        }
        public static bool PlayerHasDreamDash(Entity e)
        {
            return DreamBlockConfigurer.dreamblock_enabled(e);
        }
        public static bool? ConfigureReverse(Entity e, bool? v)
        {
            return obsoleteMode(e, v, DreamBlockConfigFlags.reverse);
        }
        public static bool? ConfigureEnable(Entity e, bool? v)
        {
            return obsoleteMode(e, v, DreamBlockConfigFlags.alwaysEnable);
        }
        public static bool? ConfigureDisable(Entity e, bool? v)
        {
            return obsoleteMode(e, v, DreamBlockConfigFlags.alwaysDisable);
        }
        public static bool? ConfigureHighPriority(Entity e, bool? v)
        {
            return obsoleteMode(e, v, DreamBlockConfigFlags.highPriority);
        }
        static bool? obsoleteMode(Entity e, bool? v, DreamBlockConfigFlags flag)
        {
            if (v.HasValue)
            {
                DreamBlockConfig.GetOrAdd(e).setter(flag, v);
                return v;
            }
            else
            {
                DreamBlockConfig.TryGet(e, out var dc);
                return dc?.getter(flag);
            }
        }
        public static bool? ConfigureGetFromEnum(Entity e, long i)
        {
            DreamBlockConfig.TryGet(e, out var dc);
            return dc?.getter((DreamBlockConfigFlags)i);
        }
        public static void ConfigureSetFromEnum(Entity e, long i, bool? value)
        {
            DreamBlockConfig.GetOrAdd(e).setter((DreamBlockConfigFlags)i, value);
        }
        public static long ConfigureGetEnum(string s)
        {
            return (long)Enum.Parse<DreamBlockConfigFlags>(s);
        }
        public static ImmutableArray<List<Entity>> GetDreamBlockTrackers(Scene scene)
        {
            return DreamBlockTrackers.GetTracker(scene).ByIndex;
        }
    }

}