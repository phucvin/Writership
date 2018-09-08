using System;
using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public interface IHealth
    {
        IEl<int> Max { get; }
        IEl<int> Current { get; }
    }

    public class Health : Disposable, IHealth
    {
        public IEl<int> Max { get; private set; }
        public IEl<int> Current { get; private set; }

        public Health(IEngine engine, Info.Health info)
        {
            Max = engine.El(info.Max);
            Current = engine.El(info.Current);
        }

        public void Setup(IEngine engine,
            IEntity me, IEl<int> armorValue,
            IOp<int> tick, ILi<IModifierItem> modifiers,
            IOp<World.Actions.Hit> hit, ILi<IStickHitItem> stickHits)
        {
            cd.Add(engine.RegisterComputer(
                new object[] { modifiers, tick, hit, stickHits },
                () => ComputeCurrent(Current,
                    Max.Read(), me, armorValue.Read(),
                    tick.Read(), modifiers.Read(),
                    hit.Read(), stickHits.Read())
            ));
        }

        public static void ComputeCurrent(IEl<int> target,
            int max, IEntity me, int armorValue,
            IList<int> tick, IList<IModifierItem> modifiers,
            IList<World.Actions.Hit> hit, IList<IStickHitItem> stickHits)
        {
            if (target.Read() <= 0) return;

            int add = 0;
            int sub = 0;

            int ticks = 0;
            for (int i = 0, n = tick.Count; i < n; ++i)
            {
                ticks += tick[i];
            }

            for (int i = 0, n = modifiers.Count; i < n; ++i)
            {
                var m = modifiers[i];
                if (!(m.Info is Info.HealthCurrentModifier)) continue;
                var h = (Info.HealthCurrentModifier)modifiers[i].Info;

                add += (ticks + (m.Remain.Read() == h.Duration ? 1 : 0)) * h.Add;
            }

            for (int i = 0, n = hit.Count; i < n; ++i)
            {
                var h = hit[i];
                if (h.From.HasOwner.Owner != me) continue;

                var hitters = h.From.Hitters.Items;
                int lifeStealPercent = 0;

                for (int j = 0, m = hitters.Count; j < m; ++j)
                {
                    var l = hitters[j] as ILifeStealHitter;
                    if (l == null) continue;
                    lifeStealPercent += l.Percent;
                }

                if (lifeStealPercent > 0)
                {
                    int dealtDamage = CalcDealtDamage(hitters, h.To.Armor.Value.Read());
                    int canStealAmount = Math.Min(dealtDamage, h.To.Health.Current.Read());
                    add += (int)Math.Ceiling(canStealAmount * (lifeStealPercent / 100f));
                }
            }

            for (int i = 0, n = stickHits.Count; i < n; ++i)
            {
                var s = stickHits[i];
                var h = s.Hit;
                if (h.From.HasOwner.Owner != me) continue;

                var hitters = h.From.Hitters.Items;
                int lifeStealPercent = 0;

                for (int j = 0, m = hitters.Count; j < m; ++j)
                {
                    var l = hitters[j] as ILifeStealHitter;
                    if (l == null) continue;
                    lifeStealPercent += l.Percent;
                }

                if (lifeStealPercent > 0)
                {
                    int dealtDamage = CalcDealtDot(ticks, s, h.From.Hitters.Items, h.To.Armor.Value.Read());
                    int canStealAmount = Math.Min(dealtDamage, h.To.Health.Current.Read());
                    add += (int)Math.Ceiling(canStealAmount * (lifeStealPercent / 100f));
                }
            }

            for (int i = 0, n = hit.Count; i < n; ++i)
            {
                var h = hit[i];
                if (h.To != me) continue;
                
                sub += CalcDealtDamage(h.From.Hitters.Items, armorValue);
            }

            for (int i = 0, n = hit.Count; i < n; ++i)
            {
                var h = hit[i];
                if (h.From.HasOwner.Owner != me) continue;

                var reflectDamagePercent = h.To.DamageReflector.Percent.Read();
                if (reflectDamagePercent <= 0) continue;

                var dealtDamage = CalcDealtDamage(h.From.Hitters.Items, h.To.Armor.Value.Read());
                sub += (int)Math.Ceiling(dealtDamage * (reflectDamagePercent / 100f));
            }

            for (int i = 0, n = stickHits.Count; i < n; ++i)
            {
                var s = stickHits[i];
                var h = s.Hit;
                if (h.To != me) continue;

                sub += CalcDealtDot(ticks, s, h.From.Hitters.Items, armorValue);
            }

            // Can be very flexible here, prefer add over sub, sub over add or fair
            int current = target.Read();
            current = Math.Min(max, current + add);
            current = Math.Max(0, current - sub);
            if (current != target.Read()) target.Write(current);
        }

        public static int CalcDealtDamage(IList<IHitter> hitters, int armorValue)
        {
            int dealt = 0;

            for (int j = 0, m = hitters.Count; j < m; ++j)
            {
                var d = hitters[j] as IDamageHitter;
                var p = hitters[j] as IPureDamageHitter;
                if (d != null) dealt += d.Subtract.Read() - armorValue;
                else if (p != null) dealt += p.Subtract.Read();
            }

            return dealt;
        }

        public static int CalcDealtDot(int ticks, IStickHitItem stick, IList<IHitter> hitters, int armorValue)
        {
            int dealt = 0;

            for (int j = 0, m = hitters.Count; j < m; ++j)
            {
                var d = hitters[j] as IDotHitter;
                if (d == null) continue;

                int repeat = (stick.Elapsed.Read() % d.Speed.Read()) + ticks / d.Speed.Read();
                if (stick.Elapsed.Read() == 0 || repeat > 0)
                {
                    dealt += (d.Subtract.Read() - armorValue) * Math.Max(1, repeat);
                }
            }

            return dealt;
        }
    }
}