using System;
using Writership;

namespace Examples.SimpleBattle
{
    public interface IRandomSeed
    {
        IEl<int> Value { get; }
    }

    public class RandomSeed : Disposable, IRandomSeed
    {
        public IEl<int> Value { get; private set; }

        private readonly Random rand;

        public RandomSeed(IEngine engine, int value)
        {
            rand = new Random(value);

            Value = engine.El(rand.Next());
        }

        public void Setup(IEngine engine, IWorld world)
        {
            cd.Add(engine.RegisterComputer(
                new object[] { world.Ops.Tick },
                () =>
                {
                    if (world.Ops.Tick.Read().Count > 0)
                    {
                        Value.Write(rand.Next());
                    }
                }
            ));
        }
    }
}