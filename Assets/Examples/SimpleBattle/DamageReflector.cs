using Writership;

namespace Examples.SimpleBattle
{
    public class DamageReflector : Disposable
    {
        public readonly IEl<int> Percent;

        public DamageReflector(IEngine engine, Info.DamageReflector info)
        {
            Percent = engine.El(info.Percent);
        }

        public void Setup(IEngine engine)
        {

        }
    }
}