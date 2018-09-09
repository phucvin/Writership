using Writership;

namespace Examples.SimpleBattle
{
    public partial class World
    {
        public class Actions : Disposable
        {
            public readonly IOp<Tick> tick;
            public readonly IOp<Hit> hit;

            public Actions(IEngine engine)
            {
                tick = engine.Op<Tick>();
                hit = engine.Op<Hit>();
            }

            public void Setup(IEngine engine)
            {
            }

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
    }
}