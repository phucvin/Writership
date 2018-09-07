using Writership;

namespace Examples.SimpleBattle
{
    public interface IModifierItem
    {
        Info.IModifier Info { get; }
        IEl<int> Remain { get; }
    }

    public class ModifierItem : Disposable, IModifierItem
    {
        public Info.IModifier Info { get; private set; }
        public IEl<int> Remain { get; private set; }

        public ModifierItem(IEngine engine, Info.IModifier info)
        {
            Info = info;
            Remain = engine.El(info.Duration);
        }

        public void Setup(IEngine engine)
        {

        }
    }
}