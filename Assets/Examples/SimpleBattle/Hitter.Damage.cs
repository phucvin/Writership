using Writership;

namespace Examples.SimpleBattle
{
    public interface IDamageHitter : IHitter
    {
        IEl<int> Subtract { get; }
        IEl<int> PureChance { get; }
        IEl<int> CriticalChance { get; }
        IEl<int> LifeStealPercent { get; }
        IEl<int> DotSpeed { get; }
    }

    public class DamageHitter : Hitter, IDamageHitter
    {
        public IEl<int> Subtract { get; private set; }
        public IEl<int> PureChance { get; private set; }
        public IEl<int> CriticalChance { get; private set; }
        public IEl<int> LifeStealPercent { get; private set; }
        public IEl<int> DotSpeed { get; private set; }

        public DamageHitter(IEngine engine, Info.DamageHitter info)
        {
            Subtract = engine.El(info.Subtract);
            PureChance = engine.El(info.PureChance);
            CriticalChance = engine.El(info.CriticalChance);
            LifeStealPercent = engine.El(info.LifeStealPercent);
            DotSpeed = engine.El(info.DotSpeed);
        }

        public void Setup(IEngine engine)
        {
            // TODO Handle buffs (modifiers)
        }

        public DamageHitter Instantiate(IEngine engine)
        {
            var instance = new DamageHitter(engine, new Info.DamageHitter
            {
                Subtract = Subtract.Read()
            });

            // TODO Setup compute for instance's fields

            return instance;
        }
    }
}