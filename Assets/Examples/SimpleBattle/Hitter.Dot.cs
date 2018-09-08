using Writership;

namespace Examples.SimpleBattle
{
    public interface IDotHitter : IHitter
    {
        IEl<int> Subtract { get; }
        IEl<int> Speed { get; }
    }

    public class DotHitter : Hitter, IDotHitter
    {
        public IEl<int> Subtract { get; private set; }
        public IEl<int> Speed { get; private set; }

        public DotHitter(IEngine engine, Info.DotHitter info)
        {
            Subtract = engine.El(info.Subtract);
            Speed = engine.El(info.Speed);
        }

        public void Setup(IEngine engine)
        {

        }

        public Hitter Instantiate(IEngine engine)
        {
            var instance = new DotHitter(engine, new Info.DotHitter
            {
                Subtract = Subtract.Read(),
                Speed = Speed.Read()
            });

            // TODO Setup compute for instance's fields

            return instance;
        }
    }
}