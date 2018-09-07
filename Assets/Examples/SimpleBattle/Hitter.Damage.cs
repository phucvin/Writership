using Writership;

namespace Examples.SimpleBattle
{
    public class DamageHitter : Hitter
    {
        public readonly IEl<int> Subtract;

        public DamageHitter(IEngine engine, Info.DamageHitter info)
        {
            Subtract = engine.El(info.Subtract);
        }

        public void Setup(IEngine engine)
        {

        }

        public Hitter Instantiate(IEngine engine)
        {
            var instance = new DamageHitter(engine, new Info.DamageHitter
            {
                Subtract = Subtract.Read()
            });

            // TODO Setup compute for instance's fields

            return instance;
        }
    }
}