namespace Writership
{
    public class ElWithRaw<T, TRaw> : El<T>
    {
        public readonly El<TRaw> Raw;

        public ElWithRaw(IEngine engine, T value, TRaw rawValue = default(TRaw))
            : base(engine, value)
        {
            Raw = new El<TRaw>(engine, rawValue);
        }
    }
}
