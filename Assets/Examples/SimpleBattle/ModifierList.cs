using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public class ModifierList : Disposable
    {
        public readonly ILi<ModifierItem> List;

        public ModifierList(IEngine engine)
        {
            List = engine.Li(new List<ModifierItem>());
        }

        public void Setup(IEngine engine)
        {

        }
    }
}