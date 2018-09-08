﻿using System.Collections.Generic;
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

    public interface IStickHitItem
    {
        World.Actions.Hit Hit { get; }
        IEl<int> Elapsed { get; }
    }

    public class StickHitItem : Disposable, IStickHitItem
    {
        public World.Actions.Hit Hit { get; private set; }
        public IEl<int> Elapsed { get; private set; }

        public StickHitItem(IEngine engine, World.Actions.Hit hit)
        {
            Hit = hit;
            Elapsed = engine.El(0);
        }

        public void Setup(IEngine engine, IOp<int> tick)
        {
            cd.Add(engine.RegisterComputer(
                new object[] { tick },
                () => ComputeElapsed(Elapsed, tick.Read())
            ));
        }

        public static void ComputeElapsed(IEl<int> target, IList<int> tick)
        {
            int elapsed = target.Read();

            for (int i = 0, n = tick.Count; i < n; ++i)
            {
                elapsed += tick[i];
            }

            if (elapsed != target.Read()) target.Write(elapsed);
        }
    }
}