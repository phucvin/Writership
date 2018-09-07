using Writership;

namespace Examples.SimpleBattle
{
    public interface ILifeStealHitter : IHitter
    {
        int Percent { get; }
    }

    public class LifeStealHitter : Hitter, ILifeStealHitter
    {
        public int Percent { get; private set; }

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