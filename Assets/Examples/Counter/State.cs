using Writership;

namespace Examples.Counter
{
    public class State
    {
        public readonly IEl<int> Value;
        public readonly IOp<Empty> Increase;
        public readonly IOp<Empty> Decrease;

        public State(CompositeDisposable cd, IEngine engine)
        {
            Value = engine.El(0);
            Increase = engine.Op<Empty>(allowMulticast: true);
            Decrease = engine.Op<Empty>(allowMulticast: true);

            engine.Computer(cd,
                new object[] {
                    Increase,
                    Decrease
                },
                () => Computers.Value(
                    Value,
                    Increase,
                    Decrease
                )
            );
        }
    }
}