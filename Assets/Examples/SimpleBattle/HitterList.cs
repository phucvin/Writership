using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public interface IHitterList
    {
        IList<IHitter> Items { get; }
    }

    public class HitterList : Disposable, IHitterList
    {
        public IList<IHitter> Items { get; private set; }

        public HitterList(IEngine engine, IList<Info.IHitter> info)
        {
            var items = new List<IHitter>();
            for (int i = 0, n = info.Count; i < n; ++i)
            {
                items.Add(Hitter.PolyNew(engine, info[i]));
            }
            Items = items.AsReadOnly();
        }

        public void Setup(IEngine engine)
        {
            for (int i = 0, n = Items.Count; i < n; ++i)
            {
                Hitter.PolySetup(Items[i], engine);
            }
        }
    }
}