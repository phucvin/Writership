using System.Collections.Generic;
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
        private readonly Info.DamageHitter info;
        public IEl<int> Subtract { get; private set; }
        public IEl<int> PureChance { get; private set; }
        public IEl<int> CriticalChance { get; private set; }
        public IEl<int> LifeStealPercent { get; private set; }
        public IEl<int> DotSpeed { get; private set; }

        public DamageHitter(IEngine engine, Info.DamageHitter info)
            : base(info)
        {
            this.info = info;
            Subtract = engine.El(info.Subtract);
            PureChance = engine.El(info.PureChance);
            CriticalChance = engine.El(info.CriticalChance);
            LifeStealPercent = engine.El(info.LifeStealPercent);
            DotSpeed = engine.El(info.DotSpeed);
        }

        public DamageHitter Instantiate(IEngine engine)
        {
            var instance = new DamageHitter(engine, new Info.DamageHitter
            {
                Subtract = Subtract.Read(),
                PureChance = PureChance.Read(),
                CriticalChance = CriticalChance.Read(),
                LifeStealPercent = LifeStealPercent.Read(),
                DotSpeed = DotSpeed.Read(),
            });

            instance.SetupForInstantiate();

            return instance;
        }

        public void Setup(CompositeDisposable cd, IEngine engine, IEntity entity)
        {
            cd.Add(engine.RegisterComputer(
                new object[] { entity.Modifiers.Items },
                () => ComputeCriticalChance(CriticalChance, info.CriticalChance,
                    entity.Modifiers.Items.Read())
            ));

            // Can do the same for Subtract, PureChance, ...
        }

        private void SetupForInstantiate()
        {
            // Can setup compute for instance's fields
            // to behave differently from original
        }

        public static void ComputeCriticalChance(IEl<int> target, int baseCriticalChance,
            IList<IModifierItem> modifiers)
        {
            int add = 0;

            for (int i = 0, n = modifiers.Count; i < n; ++i)
            {
                var m = modifiers[i];
                if (!(m.Info is Info.DamageCriticalChanceModifier)) continue;
                var a = (Info.DamageCriticalChanceModifier)m.Info;
                add += a.Add;
            }

            int criticalChance = baseCriticalChance;
            criticalChance += add;
            if (criticalChance != target.Read()) target.Write(criticalChance);
        }
    }
}