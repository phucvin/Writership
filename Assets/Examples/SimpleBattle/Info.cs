namespace Examples.SimpleBattle.Info
{
    public struct Team
    {
        public int Value;
    }

    public struct Health
    {
        public int Max;
        public int Current;
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
    }
    
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

    public struct Character
    {
        public Team Team;
        public Health Health;
        public Armor Armor;
        public DamageReflector DamageReflector;
        public IModifier[] Modifiers;
    }

    public struct Bullet
    {
        public IHitter[] Hitters;
    }
}