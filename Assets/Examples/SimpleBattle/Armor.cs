using Writership;

namespace Examples.SimpleBattle
{
    public interface IAmor
    {
        IEl<int> Value { get; }
    }

    public class Armor : Disposable, IAmor
    {
        public IEl<int> Value { get; private set; }

        public Armor(IEngine engine, Info.Armor info)
        {
            Value = engine.El(info.Value);
        }

        public void Setup(IEngine engine)
        {

        }
    }
}