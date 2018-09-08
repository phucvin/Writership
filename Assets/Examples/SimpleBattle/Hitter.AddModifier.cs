using Writership;

namespace Examples.SimpleBattle
{
    public interface IAddModifierHitter : IHitter
    {
        Info.IModifier Modifier { get; }
    }

    public class AddModifierHitter : Hitter, IAddModifierHitter
    {
        public Info.IModifier Modifier { get; private set; }

        public AddModifierHitter(IEngine engine, Info.AddModifierHitter info)
        {
            Modifier = info.Modifer;
        }

        public void Setup(IEngine engine)
        {

        }

        public Hitter Instantiate()
        {
            return this;
        }
    }
}