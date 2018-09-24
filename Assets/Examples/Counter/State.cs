using Writership;

namespace Examples.Counter
{
    public class State
    {
        public readonly IEl<int> Value;
        public readonly IMultiOp<Empty> Increase;
        public readonly IMultiOp<Empty> Decrease;

        public State(CompositeDisposable cd, IEngine engine)
        {
            Value = engine.El(0);
            Increase = engine.MultiOp<Empty>();
            Decrease = engine.MultiOp<Empty>();

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