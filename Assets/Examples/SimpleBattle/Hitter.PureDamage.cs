using Writership;

namespace Examples.SimpleBattle
{
    public interface IPureDamageHitter : IHitter
    {
        IEl<int> Subtract { get; }
    }

    public class PureDamageHitter : Hitter, IPureDamageHitter
    {
        public IEl<int> Subtract { get; private set; }

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