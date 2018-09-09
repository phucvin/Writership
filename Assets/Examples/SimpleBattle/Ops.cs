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
        IOp<Ops.Tick> Tick { get; }
        IOp<Ops.Hit> Hit { get; }
        IOp<Ops.EndHit> EndHit { get; }
    }

    public class Ops_ : Disposable, IOps
    {
        public IOp<Ops.Tick> Tick { get; private set; }
        public IOp<Ops.Hit> Hit { get; private set; }
        public IOp<Ops.EndHit> EndHit { get; private set; }

        public Ops_(IEngine engine)
        {
            Tick = engine.Op<Ops.Tick>();
            Hit = engine.Op<Ops.Hit>();
            EndHit = engine.Op<Ops.EndHit>();
        }

        public void Setup(IEngine engine)
        {
        }
    }
}