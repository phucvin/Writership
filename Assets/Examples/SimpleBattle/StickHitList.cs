using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public interface IStickHitList
    {
        ILi<IStickHitItem> Hits { get; }
    }

    public class StickHitList : Disposable, IStickHitList
    {
        public ILi<IStickHitItem> Hits { get; private set; }

        public StickHitList(IEngine engine)
        {
            Hits = engine.Li(new List<IStickHitItem>());
        }

        public void Setup(IEngine engine,
            IOp<World.Actions.Hit> hit, IOp<World.Actions.EndHit> endHit)
        {

        }
    }
}