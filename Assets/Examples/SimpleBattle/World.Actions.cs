using Writership;

namespace Examples.SimpleBattle
{
    public partial class World
    {
        public class Actions
        {
            public struct Hit
            {
                public EntityId FromOwner;
                public ILi<Hitter> FromHitters;

                public EntityId To;
                public IEl<int> ToArmorValue;
                public IEl<int> ToReflectDamagePercent;
                public IEl<int> ToHealthCurrent;
            }
        }
    }
}