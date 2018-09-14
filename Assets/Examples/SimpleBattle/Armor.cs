using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public interface IArmor
    {
        IEl<int> Value { get; }
    }

    public class Armor : IArmor
    {
        private readonly int baseValue;
        public IEl<int> Value { get; private set; }

        public Armor(IEngine engine, Info.Armor info)
        {
            baseValue = info.Value;
            Value = engine.El(baseValue);
        }

        public void Setup(CompositeDisposable cd, IEngine engine, IEntity entity)
        {
            engine.Computer(cd,
                new object[] { entity.Modifiers.Items },
                () => ComputeValue(Value, baseValue,
                    entity.Modifiers.Items.Read())
            );
        }

        public static void ComputeValue(IEl<int> target, int baseValue,
            IList<IModifierItem> modifiers)
        {
            int add = 0;
            int multiply = 0;

            for (int i = 0, n = modifiers.Count; i < n; ++i)
            {
                var m = modifiers[i];
                if (!(m.Info is Info.ArmorModifier)) continue;
                var a = (Info.ArmorModifier)m.Info;
                add += a.Add;
                multiply += a.Multiply;
            }

            int value = baseValue;
            if (multiply != 0) value *= multiply;
            value += add;
            if (value != target.Read()) target.Write(value);
        }
    }
}