using Writership;

namespace Examples.SimpleBattle
{
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

        public Hitter Instantiate()
        {
            return this;
        }
    }
}