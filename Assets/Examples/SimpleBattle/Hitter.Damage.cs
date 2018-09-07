using Writership;

namespace Examples.SimpleBattle
{
    public interface IDamageHitter : IHitter
    {
        IEl<int> Subtract { get; }
    }

    public class DamageHitter : Hitter, IDamageHitter
    {
        public IEl<int> Subtract { get; private set; }

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