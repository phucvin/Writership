using System;
using Writership;

namespace Examples.SimpleBattle
{
    public abstract class Hitter : Disposable
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
            else if (info is Info.PureDamageHitter)
            {
                return new PureDamageHitter(engine, (Info.PureDamageHitter)info);
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
            else if (self is PureDamageHitter)
            {
                ((PureDamageHitter)self).Setup(engine);
            }
            else throw new NotImplementedException();
        }

        public static Hitter PolyInstantiate(Hitter self, IEngine engine)
        {
            if (self is DamageHitter)
            {
                return ((DamageHitter)self).Instantiate(engine);
            }
            else if (self is LifeStealHitter)
            {
                return ((LifeStealHitter)self).Instantiate();
            }
            else if (self is PureDamageHitter)
            {
                return ((PureDamageHitter)self).Instantiate(engine);
            }
            else throw new NotImplementedException();
        }
    }
}