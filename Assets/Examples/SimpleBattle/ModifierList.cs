using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public class ModifierList : Disposable
    {
        public readonly ILi<IModifierItem> List;

        public ModifierList(IEngine engine)
        {
            List = engine.Li(new List<IModifierItem>());
        }

        public void Setup(IEngine engine)
        {

        }
    }
}