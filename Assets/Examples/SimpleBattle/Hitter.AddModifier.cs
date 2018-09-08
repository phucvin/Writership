using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public interface IAddModifierHitter : IHitter
    {
        IList<Info.IModifier> Modifiers { get; }
    }

    public class AddModifierHitter : Hitter, IAddModifierHitter
    {
        public IList<Info.IModifier> Modifiers { get; private set; }

        public AddModifierHitter(IEngine engine, Info.AddModifierHitter info)
        {
            Modifiers = info.Modifers;
        }

        public void Setup(IEngine engine)
        {

        }

        public AddModifierHitter Instantiate()
        {
            return this;
        }
    }
}