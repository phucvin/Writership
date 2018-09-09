using Writership;

namespace Examples.SimpleBattle
{
    public interface IDamageReflector
    {
        IEl<int> Percent { get; }
    }

    public class DamageReflector : Disposable, IDamageReflector
    {
        public IEl<int> Percent { get; private set; }

        public DamageReflector(IEngine engine, Info.DamageReflector info)
        {
            Percent = engine.El(info.Percent);
        }

        public void Setup(IEngine engine)
        {
            // TODO
        }
    }
}