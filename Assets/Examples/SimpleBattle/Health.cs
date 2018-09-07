using System;
using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public class Health : Disposable
    {
        public readonly Info.Health Info;
        public readonly IEl<int> Max;
        public readonly IEl<int> Current;

        public Health(IEngine engine, Info.Health info)
        {
            Info = info;
            Max = engine.El(info.Max);
            Current = engine.El(info.Current);
        }

        public void Setup(IEngine engine,
            EntityId me, IEl<int> armorValue,
            IOp<int> tick, ILi<ModifierItem> modifiers,
            IOp<World.Actions.Hit> hit)
        {
            cd.Add(engine.RegisterComputer(
                new object[] { modifiers, tick },
                () => ComputeCurrent(Current,
                    Max.Read(), me, armorValue.Read(),
                    tick.Read(), modifiers.Read(), hit.Read())
            ));
        }

        public static void ComputeCurrent(IEl<int> target,
            int max, EntityId me, int armorValue,
            IList<int> tick, IList<ModifierItem> modifiers,
            IList<World.Actions.Hit> hit)
        {
            int current = target.Read();

            if (current <= 0) return;

            int ticks = 0;
            for (int i = 0, n = tick.Count; i < n; ++i)
            {
                ticks += tick[i];
            }

            for (int i = 0, n = modifiers.Count; i < n; ++i)
            {
                var m = modifiers[i].Info as Info.HealthCurrentModifier?;
                if (m == null) continue;

                current += ticks * m.Value.Add;
            }

            for (int i = 0, n = hit.Count; i < n; ++i)
            {
                var h = hit[i];
                if (h.FromOwner != me) continue;
                
                var hitters = h.FromHitters.Read();
                int lifeStealPercent = 0;

                for (int j = 0, m = hitters.Count; j < m; ++j)
                {
                    var l = hitters[j] as LifeStealHitter;
                    if (l == null) continue;
                    lifeStealPercent += l.Percent;
                }

                if (lifeStealPercent > 0)
                {
                    int dealtDamage = CalcDealtDamage(hitters, h.ToArmorValue.Read());
                    int canStealAmount = Math.Min(dealtDamage, h.ToHealthCurrent.Read());

                    current += (int)Math.Ceiling(canStealAmount * (lifeStealPercent / 100f));
                }
            }

            for (int i = 0, n = hit.Count; i < n; ++i)
            {
                var h = hit[i];
                if (h.To != me) continue;
                
                current -= CalcDealtDamage(h.FromHitters.Read(), armorValue);
            }

            for (int i = 0, n = hit.Count; i < n; ++i)
            {
                var h = hit[i];
                if (h.FromOwner != me) continue;

                var reflectDamagePercent = h.ToReflectDamagePercent.Read();
                if (reflectDamagePercent <= 0) continue;

                var dealtDamage = CalcDealtDamage(h.FromHitters.Read(), h.ToArmorValue.Read());
                current -= (int)Math.Ceiling(dealtDamage * (reflectDamagePercent / 100f));
            }

            if (current > max) current = max;

            if (current != target.Read()) target.Write(current);
        }

        public static int CalcDealtDamage(IList<Hitter> hitters, int armorValue)
        {
            int damage = 0;

            for (int j = 0, m = hitters.Count; j < m; ++j)
            {
                var d = hitters[j] as DamageHitter;
                var p = hitters[j] as PureDamageHitter;
                if (d != null) damage += d.Subtract.Read() - armorValue;
                else if (p != null) damage += p.Subtract.Read();
            }

            return damage;
        }
    }
}