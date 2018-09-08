using Writership;

namespace Examples.SimpleBattle
{
    public partial class World
    {
        public class Actions
        {
            public struct Tick
            {
                public int Dt;
            }

            public struct Hit
            {
                public IEntity From;
                public IEntity To;
                public RandomSeeds RandomSeeds;
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