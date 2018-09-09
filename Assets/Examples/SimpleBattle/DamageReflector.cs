using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public interface IDamageReflector
    {
        IEl<int> Percent { get; }
    }

    public class DamageReflector : Disposable, IDamageReflector
    {
        private readonly int basePercent;
        public IEl<int> Percent { get; private set; }

        public DamageReflector(IEngine engine, Info.DamageReflector info)
        {
            basePercent = info.Percent;
            Percent = engine.El(basePercent);
        }

        public void Setup(IEngine engine, IEntity entity)
        {
            cd.Add(engine.RegisterComputer(
                new object[] { entity.Modifiers.Items },
                () => ComputePercent(Percent, basePercent,
                    entity.Modifiers.Items.Read())
            ));
        }

        public static void ComputePercent(IEl<int> target, int basePercent,
            IList<IModifierItem> modifiers)
        {
            int add = 0;

            for (int i = 0, n = modifiers.Count; i < n; ++i)
            {
                var m = modifiers[i];
                if (!(m.Info is Info.DamageReflectorModifier)) continue;
                var a = (Info.DamageReflectorModifier)m.Info;
                add += a.AddPercent;
            }

            int percent = basePercent;
            percent += add;
            if (percent != target.Read()) target.Write(percent);
        }
    }
}