using Writership;

namespace Examples.SimpleBattle
{
    public class ModifierItem : Disposable
    {
        public Info.IModifier Info;
        public readonly IEl<int> Remain;

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