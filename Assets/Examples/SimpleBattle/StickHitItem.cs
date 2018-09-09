﻿using System;
using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public interface IStickHitItem
    {
        Ops.Hit Hit { get; }
        IEl<int> Elapsed { get; }
    }

    public interface IStickHitItemFactory
    {
        IStickHitItem Create(Ops.Hit hit);
    }

    public class StickHitItem : Disposable, IStickHitItem
    {
        public Ops.Hit Hit { get; private set; }
        public IEl<int> Elapsed { get; private set; }

        public StickHitItem(IEngine engine, Ops.Hit hit)
        {
            Hit = hit;
            Elapsed = engine.El(0);
        }

        public void Setup(IEngine engine, IWorld world)
        {
            cd.Add(engine.RegisterComputer(
                new object[] { world.Ops.Tick },
                () => ComputeElapsed(Elapsed, world.Ops.Tick.Read())
            ));
        }

        public static void ComputeElapsed(IEl<int> target,
            IList<Ops.Tick> tick)
        {
            int elapsed = target.Read();

            for (int i = 0, n = tick.Count; i < n; ++i)
            {
                elapsed += tick[i].Dt;
            }

            if (elapsed != target.Read()) target.Write(elapsed);
        }

        public class Factory : IStickHitItemFactory, IDisposable
        {
            private IEngine engine;
            private IWorld world;

            public void Setup(IEngine engine, IWorld world)
            {
                this.engine = engine;
                this.world = world;
            }

            public void Dispose()
            {
            }

            public IStickHitItem Create(Ops.Hit hit)
            {
                var item = new StickHitItem(engine, hit);
                item.Setup(engine, world);
                return item;
            }
        }
    }
}