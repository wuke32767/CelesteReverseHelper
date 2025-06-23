using Celeste.Mod.ReverseHelper.Entities;
using MonoMod.ModInterop;
using System.Collections.Immutable;

namespace Celeste.Mod.ReverseHelper
{
    //[GenerateImports("ReverseHelper.DreamBlock")]
    public static partial class DreamBlockInteropImportCallerTemplate
    {
        /// <summary>
        /// if a entity acts like DreamBlock but is not DreamBlock, use this. 
        /// it should access Inventory.DreamDash only when awaking, and save it to a field.
        /// then, it controls visuals with that field and [De]ActivateNoRoutine. 
        /// for example, Communal Helper Dream Tunnel.
        /// 
        /// anyway, better to ask me for it.
        /// actually this is not an modinterop. it's customized for the dream tunnel.
        /// </summary>
        /// [Obsolete Soon]
        public static partial void RegisterDreamBlockLike(Type targetType, Action<Entity> ActivateNoRoutine, Action<Entity> DeactivateNoRoutine);
        /// <summary>
        /// if a entity uses an DreamBlock dummy to listen to dream inventory, use this.
        /// for example, Communal Helper Dream Tunnel.
        /// 
        /// anyway, better to ask me for it.
        /// actually this is not an modinterop. it's customized for the dream tunnel.
        /// </summary>
        public static partial void RegisterDreamBlockDummy(Type targetType, Func<Entity, Entity> GetDummy);
        /// <summary> 
        /// or, use this to check if your entity is enabled.
        /// notice that because of awake order, your entity might awake before reversed.
        /// better to awake at the end of awake frame.
        /// </summary>
        /// <param name="e">the entity to be checked.</param>
        /// <param name="fallback">if ReverseHelper is not loaded, use fallback instead.</param>
        public static partial bool PlayerHasDreamDash(Entity e, Func<bool>? fallback = null);
        /// <summary>
        /// generic version of these options.
        /// https://github.com/wuke32767/CelesteReverseHelper/blob/a4919894497bc501be7f9f8f5c08923a1187af1c/Src/Entities/DreamBlock/DreamBlockConfigurer.cs#L15
        /// those option name and value will not be changed (wip excluded), so you can just hardcode them.
        /// getter.
        /// </summary>
        /// <returns>
        /// if this flag is set.
        /// null: not set / not loaded.
        /// not null: get flag.
        /// </returns>
        public static partial bool? ConfigureGetFromEnum(Entity e, long i);
        /// <summary>
        /// generic version of these option.
        /// setter.
        /// </summary>
        /// notice that it can clear a flag.
        public static partial void ConfigureSetFromEnum(Entity e, long i, bool? value);
        /// <summary>
        /// get option from string.
        /// </summary>
        /// <returns> one of Enum.GetValues<DreamBlockConfigFlags>() </returns>
        /// (param) one of Enum.GetNames<DreamBlockConfigFlags>()
        public static partial long ConfigureGetEnum(string s);
        /// <summary>
        /// returns trackers for these flags.
        /// </summary>
        /// <returns>
        /// index it with the enum and you would get all dreamblock that has the flag.
        /// sometimes it contains Dream Tunnel [Communal Helper], which is not DreamBlock. 
        /// </returns>
        public static partial ImmutableArray<List<Entity>>? GetDreamBlockTrackers(Scene scene);
    }
    public static partial class DreamBlockInteropImportCallerTemplate
    {
        public static partial void RegisterDreamBlockLike(Type targetType, Action<Entity> ActivateNoRoutine, Action<Entity> DeactivateNoRoutine)
        {
            DreamBlockInteropImportTemplate.RegisterDreamBlockLike?.Invoke(targetType, ActivateNoRoutine, DeactivateNoRoutine);
        }
        public static partial void RegisterDreamBlockDummy(Type targetType, Func<Entity, Entity> GetDummy)
        {
            DreamBlockInteropImportTemplate.RegisterDreamBlockDummy?.Invoke(targetType, GetDummy);
        }
        public static partial bool PlayerHasDreamDash(Entity e, Func<bool>? fallback = null)
        {
            if (DreamBlockInteropImportTemplate.PlayerHasDreamDash is null)
            {
                return fallback?.Invoke() ?? (Engine.Scene as Level)?.Session?.Inventory.DreamDash ?? false;
            }
            return DreamBlockInteropImportTemplate.PlayerHasDreamDash(e);
        }
        public static partial bool? ConfigureGetFromEnum(Entity e, long i)
        {
            return DreamBlockInteropImportTemplate.ConfigureGetFromEnum?.Invoke(e, i);
        }
        public static partial void ConfigureSetFromEnum(Entity e, long i, bool? value)
        {
            DreamBlockInteropImportTemplate.ConfigureSetFromEnum?.Invoke(e, i, value);
        }
        public static partial long ConfigureGetEnum(string s)
        {
            return DreamBlockInteropImportTemplate.ConfigureGetEnum?.Invoke(s) ?? 0;
        }
        public static partial ImmutableArray<List<Entity>>? GetDreamBlockTrackers(Scene scene)
        {
            return DreamBlockInteropImportTemplate.GetDreamBlockTrackers?.Invoke(scene);
        }
    }

    [ModImportName("ReverseHelper.DreamBlock")]
    public static class DreamBlockInteropImportTemplate
    {
        public static Action<Type, Func<Entity, Entity>>? RegisterDreamBlockDummy;
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
        public static void RegisterDreamBlockDummy(Type targetType, Func<Entity, Entity> GetDummy)
        {
            if (targetType is not null && GetDummy is not null)
            {
                DreamBlockConfigurer.ExternalDreamBlockDummy[targetType] = GetDummy;
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