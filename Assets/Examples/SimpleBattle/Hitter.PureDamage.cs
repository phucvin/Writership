using Writership;

namespace Examples.SimpleBattle
{
    public class PureDamageHitter : Hitter
    {
        public readonly IEl<int> Subtract;

        public PureDamageHitter(IEngine engine, Info.PureDamageHitter info)
        {
            Subtract = engine.El(info.Subtract);
        }

        public void Setup(IEngine engine)
        {

        }

        public Hitter Instantiate(IEngine engine)
        {
            var instance = new PureDamageHitter(engine, new Info.PureDamageHitter
            {
                Subtract = Subtract.Read()
            });

            // TODO Setup compute for instance's fields

            return instance;
        }
    }
}