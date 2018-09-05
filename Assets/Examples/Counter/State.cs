using Writership;

namespace Examples.Counter
{
    // TODO Disposable
    public class State
    {
        public readonly El<int> Value;
        public readonly Op<Void> Increase;
        public readonly Op<Void> Decrease;

        public State(Engine engine)
        {
            Value = engine.El(0);
            Increase = engine.Op<Void>();
            Decrease = engine.Op<Void>();

            engine.RegisterComputer(
                new object[] { Increase, Decrease, },
                () => Computers.Value(Value, Increase, Decrease)
            );
        }
    }
}