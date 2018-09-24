using Writership;

namespace Examples.SimpleBattle
{
    public static class Ops
    {
        public struct Tick
        {
            public int Dt;
        }

        public struct Hit
        {
            public IEntity From;
            public IEntity To;
            public int RandomSeed;
        }

        public struct EndHit
        {
            public IEntity From;
            public IEntity To;
        }

        // TODO Handle this
        public struct Cast
        {
            public IEntity Owner;
            public ILi<IHitter> Hitters;
        }
    }

    public interface IOps
    {
        IMultiOp<Ops.Tick> Tick { get; }
        IMultiOp<Ops.Hit> Hit { get; }
        IMultiOp<Ops.EndHit> EndHit { get; }
    }

    public class Ops_ : IOps
    {
        public IMultiOp<Ops.Tick> Tick { get; private set; }
        public IMultiOp<Ops.Hit> Hit { get; private set; }
        public IMultiOp<Ops.EndHit> EndHit { get; private set; }

        public Ops_(IEngine engine)
        {
            Tick = engine.MultiOp<Ops.Tick>();
            Hit = engine.MultiOp<Ops.Hit>();
            EndHit = engine.MultiOp<Ops.EndHit>();
        }

        public void Setup()
        {
        }
    }
}