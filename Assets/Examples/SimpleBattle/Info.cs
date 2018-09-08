namespace Examples.SimpleBattle.Info
{
    public struct Health
    {
        public int Max;
        public int Current;
        public int RegenSpeed;
    }

    public struct Armor
    {
        public int Value;
    }

    public struct DamageReflector
    {
        public int Percent;
    }

    public interface IModifier
    {
        int Duration { get; }
    }

    public struct HealthCurrentModifier : IModifier
    {
        public int Add;
        public int Duration { get; set; }
    }

    public struct DamageReflectorModifier : IModifier
    {
        public int Add;
        public int Duration { get; set; }
    }

    public interface IHitter
    {
    }

    public struct DamageHitter : IHitter
    {
        public int Subtract;
        // TODO Use this in calculating dealt damage
        // TODO Consider even make it independent from hitters
        public int CoeffPercent;
    }

    // TODO Merge PureDamage to Damage too
    // TODO Critical hit -> Hard because of random is different on different compute location
    public struct PureDamageHitter : IHitter
    {
        public int Subtract;
    }

    public struct LifeStealHitter : IHitter
    {
        public int Percent;
    }

    public struct AddModifierHitter : IHitter
    {
        public IModifier Modifer;
    }

    public struct DotHitter : IHitter
    {
        public int Subtract;
        public int Speed;
    }
}