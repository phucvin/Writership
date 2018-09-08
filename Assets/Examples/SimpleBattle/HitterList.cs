using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public interface IHitterList
    {
        IList<IHitter> Hitters { get; }
    }

    public class HitterList : Disposable
    {
        public IList<IHitter> Hitters { get; private set; }

        private readonly List<IHitter> hitters;

        public HitterList(IEngine engine, IList<Info.IHitter> info)
        {
            hitters = new List<IHitter>();
            Hitters = hitters.AsReadOnly();

            for (int i = 0, n = info.Count; i < n; ++i)
            {
                hitters.Add(Hitter.PolyNew(engine, info[i]));
            }
        }

        public void Setup(IEngine engine)
        {
            for (int i = 0, n = hitters.Count; i < n; ++i)
            {
                Hitter.PolySetup(hitters[i], engine);
            }
        }
    }
}