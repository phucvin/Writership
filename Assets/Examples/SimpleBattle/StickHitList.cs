using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public interface IStickHitList
    {
        ILi<IStickHitItem> Items { get; }
    }

    public class StickHitList : IStickHitList
    {
        public ILi<IStickHitItem> Items { get; private set; }

        public StickHitList(IEngine engine)
        {
            Items = engine.Li(new List<IStickHitItem>());
        }

        public void Setup(CompositeDisposable cd, IEngine engine, IWorld world)
        {
            engine.Computer(cd,
                new object[] { world.Ops.Hit, world.Ops.EndHit },
                () => ComputeItems(Items,
                    world.Ops.Hit.Read(), world.Ops.EndHit.Read(),
                    world.StickHitItemFactory)
            );
        }

        public static void ComputeItems(ILi<IStickHitItem> target,
            IList<Ops.Hit> hit, IList<Ops.EndHit> endHit,
            IStickHitItemFactory itemFactory)
        {
            if (hit.Count <= 0 && endHit.Count <= 0) return;

            var items = target.AsWrite();

            for (int i = 0, n = hit.Count; i < n; ++i)
            {
                items.Add(itemFactory.Create(hit[i]));
            }

            if (endHit.Count > 0)
            {
                for (int i = items.Count - 1; i >= 0; --i)
                {
                    for (int j = 0, m = endHit.Count; j < m; ++j)
                    {
                        var start = items[i].Hit;
                        var end = endHit[j];
                        if (start.From == end.From && start.To == end.To)
                        {
                            itemFactory.Dispose(items[i]);
                            items.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
}