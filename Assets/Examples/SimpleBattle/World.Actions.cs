using Writership;

namespace Examples.SimpleBattle
{
    public partial class World
    {
        public class Actions
        {
            public struct Hit
            {
                public Entity From;
                public Entity FromOwner;
                public ILi<IHitter> FromHitters;

                public Entity To;
                public IEl<int> ToArmorValue;
                public IEl<int> ToReflectDamagePercent;
                public IEl<int> ToHealthCurrent;
            }

            public struct EndHit
            {
                public Entity From;
                public Entity FromOwner;

                public Entity To;
            }

            // TODO Handle this
            public struct Cast
            {
                public Entity Owner;
                public ILi<IHitter> Hitters;
            }
        }
    }
}