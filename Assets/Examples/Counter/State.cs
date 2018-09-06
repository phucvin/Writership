using System;
using Writership;

namespace Examples.Counter
{
    public class State : IDisposable
    {
        public readonly IEl<int> Value;
        public readonly IOp<Empty> Increase;
        public readonly IOp<Empty> Decrease;

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
                () => Computers.Value(
                    Value,
                    Increase,
                    Decrease
                )
            ));
        }

        public void Dispose()
        {
            cd.Dispose();
        }
    }
}