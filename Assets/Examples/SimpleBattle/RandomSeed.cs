﻿using System;
using Writership;

namespace Examples.SimpleBattle
{
    public interface IRandomSeed
    {
        IEl<int> Value { get; }
    }

    public class RandomSeed : IRandomSeed
    {
        public IEl<int> Value { get; private set; }

        private readonly Random rand;

        public RandomSeed(IEngine engine, int value)
        {
            rand = new Random(value);

            Value = engine.El(rand.Next());
        }

        public void Setup(CompositeDisposable cd, IEngine engine, IWorld world)
        {
            engine.Computer(cd,
                new object[] { world.Ops.Tick },
                () =>
                {
                    Ops.Tick tmp;
                    if (world.Ops.Tick.TryRead(out tmp))
                    {
                        Value.Write(rand.Next());
                    }
                }
            );
        }
    }
}