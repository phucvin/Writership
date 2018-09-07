using System;
using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    namespace Info
    {
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
            public int Add;
            public int Duration { get; set; }
        }

        public struct DamageReflectorModifier : IModifier
        {
            public int Add;
            public int Duration { get; set; }
        }

        public interface IHitter
        {
        }

        public struct DamageHitter : IHitter
        {
            public int Subtract;
        }

        public struct LifeStealHitter : IHitter
        {
            public int Percent;
        }
    }

    public class Cd : IDisposable
    {
        protected readonly CompositeDisposable cd = new CompositeDisposable();

        public void Dispose()
        {
            cd.Dispose();
        }
    }

    public class World
    {
        public class Actions
        {
            public struct Hit
            {
                public string FromEntityId;
                public ILi<Hitter> FromHitters;

                public string ToEntityId;
                public IEl<int> ToArmorValue;
                public IEl<int> ToReflectDamagePercent;
            }
        }
    }

    public class Character
    {

    }

    public abstract class Hitter : Cd
    {
        public static Hitter PolyNew(IEngine engine, Info.IHitter info)
        {
            if (info is Info.DamageHitter)
            {
                return new DamageHitter(engine, (Info.DamageHitter)info);
            }
            else if (info is Info.LifeStealHitter)
            {
                return new LifeStealHitter(engine, (Info.LifeStealHitter)info);
            }
            else throw new NotImplementedException();
        }

        public static void PolySetup(Hitter self, IEngine engine)
        {
            if (self is DamageHitter)
            {
                ((DamageHitter)self).Setup(engine);
            }
            else if (self is LifeStealHitter)
            {
                ((LifeStealHitter)self).Setup(engine);
            }
        }
    }

    public class DamageHitter : Hitter
    {
        public readonly IEl<int> Subtract;

        public DamageHitter(IEngine engine, Info.DamageHitter info)
        {
            Subtract = engine.El(info.Subtract);
        }

        public void Setup(IEngine engine)
        {

        }
    }

    public class LifeStealHitter : Hitter
    {
        public readonly int Percent;

        public LifeStealHitter(IEngine engine, Info.LifeStealHitter info)
        {
            Percent = info.Percent;
        }

        public void Setup(IEngine engine)
        {

        }
    }

    public class Health : Cd
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
            ILi<ModifierItem> modifiers, IOp<int> tick,
            IOp<World.Actions.Hit> hit, string entityId,
            IEl<int> armorValue)
        {
            cd.Add(engine.RegisterComputer(
                new object[] { modifiers, tick },
                () => ComputeCurrent(Current,
                    Max.Read(), modifiers.Read(), tick.Read(),
                    hit.Read(), entityId, armorValue.Read())
            ));
        }

        public static void ComputeCurrent(IEl<int> target,
            int max, IList<ModifierItem> modifiers, IList<int> tick,
            IList<World.Actions.Hit> hit, string entityId,
            int armorValue)
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
                if (h.FromEntityId != entityId) continue;
                
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
                    // TODO
                    current += (int)Math.Ceiling(dealtDamage * (lifeStealPercent / 100f));
                }
            }

            for (int i = 0, n = hit.Count; i < n; ++i)
            {
                var h = hit[i];
                if (h.ToEntityId != entityId) continue;
                
                current -= CalcDealtDamage(h.FromHitters.Read(), armorValue);
            }

            for (int i = 0, n = hit.Count; i < n; ++i)
            {
                var h = hit[i];
                if (h.FromEntityId != entityId) continue;

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
                if (d != null) damage += d.Subtract.Read();
            }

            damage -= armorValue;

            return damage;
        }
    }

    public class DamageReflector : Cd
    {
        public readonly IEl<int> Percent;

        public DamageReflector(IEngine engine, Info.DamageReflector info)
        {
            Percent = engine.El(info.Percent);
        }

        public void Setup(IEngine engine)
        {

        }
    }

    public class Armor : Cd
    {
        public readonly IEl<int> Value;

        public Armor(IEngine engine, Info.Armor info)
        {
            Value = engine.El(info.Value);
        }

        public void Setup(IEngine engine)
        {

        }
    }

    public class ModifierList : Cd
    {
        public readonly ILi<ModifierItem> List;

        public ModifierList(IEngine engine)
        {
            List = engine.Li(new List<ModifierItem>());
        }

        public void Setup(IEngine engine)
        {

        }
    }

    public class ModifierItem : Cd
    {
        public Info.IModifier Info;
        public readonly IEl<int> Remain;

        public ModifierItem(IEngine engine, Info.IModifier info)
        {
            Info = info;
            Remain = engine.El(info.Duration);
        }

        public void Setup(IEngine engine)
        {

        }
    }
}