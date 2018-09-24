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
        void Dispose(IStickHitItem item);
    }

    public class StickHitItem : IStickHitItem
    {
        public Ops.Hit Hit { get; private set; }
        public IEl<int> Elapsed { get; private set; }

        public StickHitItem(IEngine engine, Ops.Hit hit)
        {
            Hit = hit;
            Elapsed = engine.El(0);
        }

        public void Setup(CompositeDisposable cd, IEngine engine, IWorld world)
        {
            engine.Computer(cd,
                new object[] { world.Ops.Tick },
                () => ComputeElapsed(Elapsed, world.Ops.Tick)
            );
        }

        public static void ComputeElapsed(IEl<int> target,
            IOp<Ops.Tick> tick)
        {
            int elapsed = target.Read();

            Ops.Tick t;
            if (tick.TryRead(out t)) elapsed += t.Dt;

            if (elapsed != target.Read()) target.Write(elapsed);
        }

        public class Factory : CompositeDisposableFactory<IStickHitItem>, IStickHitItemFactory
        {
            private IEngine engine;
            private IWorld world;

            public void Setup(CompositeDisposable cd, IEngine engine, IWorld world)
            {
                this.engine = engine;
                this.world = world;

                cd.Add(this);
            }

            public IStickHitItem Create(Ops.Hit hit)
            {
                var item = new StickHitItem(engine, hit);
                var cd = Add(item);
                item.Setup(cd, engine, world);
                return item;
            }

            public void Dispose(IStickHitItem item)
            {
                Remove(item).Dispose();
            }
        }
    }
}