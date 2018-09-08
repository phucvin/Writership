using Writership;

namespace Examples.SimpleBattle
{
    public partial class Entity
    {
        public static Entity BuildCharacter(IEngine engine)
        {
            var e = new Entity
            {
                Health = new Health(engine, default(Info.Health)),
            };
            return e;
        }
    }
}