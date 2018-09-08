using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public interface IStickHitList
    {
        ILi<IStickHitItem> Items { get; }
    }

    public class StickHitList : Disposable, IStickHitList
    {
        public ILi<IStickHitItem> Items { get; private set; }

        public StickHitList(IEngine engine)
        {
            Items = engine.Li(new List<IStickHitItem>());
        }

        public void Setup(IEngine engine,
            IOp<World.Actions.Hit> hit, IOp<World.Actions.EndHit> endHit)
        {
            // TODO
        }
    }
}