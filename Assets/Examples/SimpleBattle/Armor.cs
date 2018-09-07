using Writership;

namespace Examples.SimpleBattle
{
    public class Armor : Disposable
    {
        public readonly IEl<int> Value;

        public Armor(IEngine engine, Info.Armor info)
        {
            Value = engine.El(info.Value);
        }

        public void Setup(IEngine engine)
        {

        }
    }
}