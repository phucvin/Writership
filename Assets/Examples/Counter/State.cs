﻿using System;
using Writership;

namespace Examples.Counter
{
    public class State : IDisposable
    {
        public readonly El<int> Value;
        public readonly Op<Empty> Increase;
        public readonly Op<Empty> Decrease;

        private readonly CompositeDisposable cd;

        public State(IEngine engine)
        {
            Value = engine.El(0);
            Increase = engine.Op<Empty>();
            Decrease = engine.Op<Empty>();

            cd = new CompositeDisposable();

            cd.Add(engine.RegisterComputer(
                new object[] {
                    Increase,
                    Decrease
                },
                // TODO Fix unnecessary mark dirty if no change
                () => Value.Write(Computers.Value(
                    Value.Read(),
                    Increase.Read(),
                    Decrease.Read()
                ))
            ));
        }

        public void Dispose()
        {
            cd.Dispose();
        }
    }
}