namespace Examples.SimpleBattle
{
    public interface IEntity
    {
        ISpatial Spatial { get; }
        ITeam Team { get; }
        IHealth Health { get; }
        IArmor Armor { get; }
        IDamageReflector DamageReflector { get; }
        IModifierList Modifiers { get; }
        Info.HitTo? HitTo { get; }
        IHitterList Hitters { get; }
        IHasOwner HasOwner { get; }
    }

    public partial class Entity : IEntity
    {
        public ISpatial Spatial { get; private set; }
        public ITeam Team { get; private set; }
        public IHealth Health { get; private set; }
        public IArmor Armor { get; private set; }
        public IDamageReflector DamageReflector { get; private set; }
        public IModifierList Modifiers { get; private set; }
        public Info.HitTo? HitTo { get; private set; }
        public IHitterList Hitters { get; private set; }
        public IHasOwner HasOwner { get; private set; }
    }
}