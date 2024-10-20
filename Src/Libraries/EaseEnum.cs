namespace Celeste.Mod.ReverseHelper.Libraries
{
    public record struct EaseGroup(Ease.Easer Easer, double EndSpeed, double MaxSpeed);

    public enum EaseEnum
    {
        Linear, SineIn, SineOut, SineInOut, QuadIn, QuadOut, QuadInOut, CubeIn, CubeOut, CubeInOut, QuintIn, QuintOut, QuintInOut, ExpoIn, ExpoOut, ExpoInOut, BackIn, BackOut, BackInOut, BigBackIn, BigBackOut, BigBackInOut, ElasticIn, ElasticOut, ElasticInOut, BounceIn, BounceOut, BounceInOut,
    }
    public static class EaseEnums
    {
        public static EaseGroup EasePlus(this EntityData e, string attr, EaseEnum def = EaseEnum.SineIn)
        {
            return FromEnum(e.Enum(attr, def));
        }
        public static EaseGroup FromEnum(EaseEnum easeEnum)
        => easeEnum switch
        {
            EaseEnum.Linear => new(Ease.Linear, 1, 1),
            EaseEnum.SineIn => new(Ease.SineIn, Math.PI / 2, Math.PI / 2),
            EaseEnum.SineOut => new(Ease.SineOut, 0, Math.PI / 2),
            EaseEnum.SineInOut => new(Ease.SineInOut, 0, Math.PI / 2),
            EaseEnum.ElasticIn => new(Ease.ElasticIn, 0, 15),
            //wip
            EaseEnum.QuadIn => new(Ease.QuadIn, 0, 0),
            EaseEnum.QuadOut => new(Ease.QuadOut, 0, 0),
            EaseEnum.QuadInOut => new(Ease.QuadInOut, 0, 0),
            EaseEnum.CubeIn => new(Ease.CubeIn, 0, 0),
            EaseEnum.CubeOut => new(Ease.CubeOut, 0, 0),
            EaseEnum.CubeInOut => new(Ease.CubeInOut, 0, 0),
            EaseEnum.QuintIn => new(Ease.QuintIn, 0, 0),
            EaseEnum.QuintOut => new(Ease.QuintOut, 0, 0),
            EaseEnum.QuintInOut => new(Ease.QuintInOut, 0, 0),
            EaseEnum.ExpoIn => new(Ease.ExpoIn, 0, 0),
            EaseEnum.ExpoOut => new(Ease.ExpoOut, 0, 0),
            EaseEnum.ExpoInOut => new(Ease.ExpoInOut, 0, 0),
            EaseEnum.BackIn => new(Ease.BackIn, 0, 0),
            EaseEnum.BackOut => new(Ease.BackOut, 0, 0),
            EaseEnum.BackInOut => new(Ease.BackInOut, 0, 0),
            EaseEnum.BigBackIn => new(Ease.BigBackIn, 0, 0),
            EaseEnum.BigBackOut => new(Ease.BigBackOut, 0, 0),
            EaseEnum.BigBackInOut => new(Ease.BigBackInOut, 0, 0),
            EaseEnum.ElasticOut => new(Ease.ElasticOut, 0, 0),
            EaseEnum.ElasticInOut => new(Ease.ElasticInOut, 0, 0),
            EaseEnum.BounceIn => new(Ease.BounceIn, 0, 0),
            EaseEnum.BounceOut => new(Ease.BounceOut, 0, 0),
            EaseEnum.BounceInOut => new(Ease.BounceInOut, 0, 0),
            _ => throw new NotImplementedException(),
        };
    }
}
