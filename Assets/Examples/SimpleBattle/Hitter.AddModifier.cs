using System.Collections.Generic;

namespace Examples.SimpleBattle
{
    public interface IAddModifierHitter : IHitter
    {
        IList<Info.IModifier> Modifiers { get; }
    }

    public class AddModifierHitter : Hitter, IAddModifierHitter
    {
        public IList<Info.IModifier> Modifiers { get; private set; }

        public AddModifierHitter(Info.AddModifierHitter info)
            : base(info)
        {
            Modifiers = info.Modifers;
        }

        public void Setup()
        {
        }

        public AddModifierHitter Instantiate()
        {
            return this;
        }
    }
}