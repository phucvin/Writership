using System;
using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public interface IHealth
    {
        IEl<int> Max { get; }
        IEl<int> Current { get; }
        IEl<int> RegenSpeed { get; }
    }

    public class Health : Disposable, IHealth
    {
        public IEl<int> Max { get; private set; }
        public IEl<int> Current { get; private set; }
        public IEl<int> RegenSpeed { get; private set; }

        public Health(IEngine engine, Info.Health info)
        {
            Max = engine.El(info.Max);
            Current = engine.El(info.Current);
            RegenSpeed = engine.El(info.RegenSpeed);
        }

        public void Setup(IEngine engine, IEntity entity, IWorld world)
        {
            cd.Add(engine.RegisterComputer(
                new object[] {
                    world.Ops.Tick,
                    world.Ops.Hit,
                    world.StickHits.Items
                },
                () => ComputeCurrent(Current, Max.Read(), RegenSpeed.Read(),
                    entity, entity.Armor.Value.Read(),
                    world.Ops.Tick.Read(), world.Ops.Hit.Read(),
                    world.StickHits.Items.Read(),
                    world.RandomSeed.Value.Read())
            ));
        }

        public static void ComputeCurrent(IEl<int> target,
            int max, int regenSpeed, IEntity entity, int armorValue,
            IList<Ops.Tick> tick, IList<Ops.Hit> hit, IList<IStickHitItem> stickHits,
            int randomSeed)
        {
            if (target.Read() <= 0) return;

            int add = 0;
            int sub = 0;

            int ticks = 0;
            for (int i = 0, n = tick.Count; i < n; ++i)
            {
                ticks += tick[i].Dt;
            }

            add += ticks * regenSpeed;

            for (int i = 0, n = hit.Count; i < n; ++i)
            {
                var h = hit[i];
                if (h.From.HasOwner.Owner != entity) continue;

                var hitters = h.From.Hitters;
                if (hitters.Damage == null) continue;

                int lifeStealPercent = hitters.Damage.LifeStealPercent.Read();
                if (lifeStealPercent <= 0) continue;

                int dealtDamage = CalcDealtDamage(hitters.Damage,
                    h.To.Armor.Value.Read(), randomSeed);
                int canStealAmount = Math.Min(dealtDamage, h.To.Health.Current.Read());
                add += (int)Math.Ceiling(canStealAmount * (lifeStealPercent / 100f));
            }

            for (int i = 0, n = stickHits.Count; i < n; ++i)
            {
                var s = stickHits[i];
                var h = s.Hit;
                if (h.From.HasOwner.Owner != entity) continue;

                var hitters = h.From.Hitters;
                if (hitters.Damage == null) continue;
                
                int lifeStealPercent = hitters.Damage.LifeStealPercent.Read();
                if (lifeStealPercent <= 0) continue;

                int dealtDamage = CalcDealtDot(ticks, s, hitters.Damage,
                    h.To.Armor.Value.Read(), randomSeed);
                int canStealAmount = Math.Min(dealtDamage, h.To.Health.Current.Read());
                add += (int)Math.Ceiling(canStealAmount * (lifeStealPercent / 100f));
            }

            for (int i = 0, n = hit.Count; i < n; ++i)
            {
                var h = hit[i];
                if (h.To != entity) continue;

                var hitters = h.From.Hitters;
                if (hitters.Damage == null) continue;

                sub += CalcDealtDamage(hitters.Damage, armorValue, randomSeed);
            }

            for (int i = 0, n = hit.Count; i < n; ++i)
            {
                var h = hit[i];
                if (h.From.HasOwner.Owner != entity) continue;

                var reflectDamagePercent = h.To.DamageReflector.Percent.Read();
                if (reflectDamagePercent <= 0) continue;

                var hitters = h.From.Hitters;
                if (hitters.Damage == null) continue;

                int dealtDamage = CalcDealtDamage(hitters.Damage,
                    h.To.Armor.Value.Read(), randomSeed);
                sub += (int)Math.Ceiling(dealtDamage * (reflectDamagePercent / 100f));
            }

            for (int i = 0, n = stickHits.Count; i < n; ++i)
            {
                var s = stickHits[i];
                var h = s.Hit;
                if (h.To != entity) continue;

                var hitters = h.From.Hitters;
                if (hitters.Damage == null) continue;

                sub += CalcDealtDot(ticks, s, hitters.Damage,
                    armorValue, randomSeed);
            }

            // Can be very flexible here, prefer add over sub, sub over add or fair
            int current = target.Read();
            current = Math.Min(max, current + add);
            current = Math.Max(0, current - sub);
            if (current != target.Read()) target.Write(current);
        }

        public static int CalcDealtDamage(IDamageHitter damage,
            int armorValue, int randomSeed)
        {
            int dealt = 0;
            Random rand;
            
            rand = new Random(randomSeed + 19042);
            if (rand.Next(100) < damage.PureChance.Read())
            {
                dealt += damage.Subtract.Read();
            }
            else
            {
                dealt += Math.Max(1, damage.Subtract.Read() - armorValue);
            }

            rand = new Random(randomSeed + 642);
            if (rand.Next(100) < damage.CriticalChance.Read())
            {
                dealt *= 2;
            }

            return dealt;
        }

        public static int CalcDealtDot(int ticks,
            IStickHitItem stick, IDamageHitter damage,
            int armorValue, int randomSeed)
        {
            int dealt = 0;
            int repeat = (stick.Elapsed.Read() % damage.DotSpeed.Read()) + ticks / damage.DotSpeed.Read();
            if (stick.Elapsed.Read() == 0 || repeat > 0)
            {
                dealt += CalcDealtDamage(damage, armorValue, randomSeed) * Math.Max(1, repeat);
            }
            return dealt;
        }
    }
}