using Writership;

namespace Examples.SimpleBattle
{
    public partial class World
    {
        public class Actions
        {
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