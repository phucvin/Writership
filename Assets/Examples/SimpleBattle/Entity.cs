namespace Examples.SimpleBattle
{
    public interface IEntity
    {
        ITeam Team { get; }
        IHealth Health { get; }
        IArmor Armor { get; }
        IDamageReflector DamageReflector { get; }
        IModifierList Modifiers { get; }
        IHitterList Hitters { get; }

    }

    public partial class Entity : IEntity
    {
        public ITeam Team { get; private set; }
        public IHealth Health { get; private set; }
        public IArmor Armor { get; private set; }
        public IDamageReflector DamageReflector { get; private set; }
        public IModifierList Modifiers { get; private set; }
        public IHitterList Hitters { get; private set; }
    }
}