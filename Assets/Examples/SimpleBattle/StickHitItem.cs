using System.Collections.Generic;
using Writership;

namespace Examples.SimpleBattle
{
    public interface IStickHitItem
    {
        Ops.Hit Hit { get; }
        IEl<int> Elapsed { get; }
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

        public void Setup(IEngine engine, IOp<Ops.Tick> tick)
        {
            cd.Add(engine.RegisterComputer(
                new object[] { tick },
                () => ComputeElapsed(Elapsed, tick.Read())
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
    }
}