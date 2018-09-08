using Writership;

namespace Examples.SimpleBattle
{
    public interface IArmor
    {
        IEl<int> Value { get; }
    }

    public class Armor : Disposable, IArmor
    {
        public IEl<int> Value { get; private set; }

        public Armor(IEngine engine, Info.Armor info)
        {
            Value = engine.El(info.Value);
        }

        public void Setup(IEngine engine)
        {
            // TODO
        }
    }
}