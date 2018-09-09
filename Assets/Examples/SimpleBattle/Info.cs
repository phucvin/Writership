using System.Collections.Generic;

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
        public int Duration { get; set; }
        public int Add;
    }

    public struct DamageReflectorModifier : IModifier
    {
        public int Duration { get; set; }
        public int AddPercent;
    }

    public struct ArmorModifier : IModifier
    {
        public int Duration { get; set; }
        public int Add;
        public int Multiply;
    }

    public struct DamageCriticalChanceModifier : IModifier
    {
        public int Duration { get; set; }
        public int Add;
    }

    public enum HitTo
    {
        Enemy,
        Teammate,
        SelfAndTeammate
    }

    public interface IHitter
    {
    }

    public struct HitterList
    {
        public DamageHitter? Damage;
        public AddModifierHitter? AddModifier;
    }

    public struct DamageHitter : IHitter
    {
        public HitTo HitTo { get; set; }
        public int Subtract;
        public int PureChance;
        public int CriticalChance;
        public int LifeStealPercent;
        public int DotSpeed;
    }

    public struct AddModifierHitter : IHitter
    {
        public HitTo HitTo { get; set; }
        public IList<IModifier> Modifers;
    }


    public struct Character
    {
        public Team Team;
        public Health Health;
        public Armor Armor;
        public DamageReflector DamageReflector;
        public IList<IModifier> Modifiers;
    }

    public struct Bullet
    {
        public HitterList Hitters;
    }
}