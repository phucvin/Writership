namespace Writership
{
    public interface ITw<T>
    {
        IHaveCells Raw { get; }
        T ReadRaw();
        T Read();
        void WriteRaw(T value);
        void Write(T value);
    }

    public class Tw<T> : ITw<T>, IHaveCells
    {
        private readonly IEngine engine;
        private readonly El<T> way1;
        private readonly El<T> way2;

        public Tw(IEngine engine, T value)
        {
            this.engine = engine;

            way1 = engine.El(value);
            way2 = engine.El(value);
        }

        public IHaveCells Raw { get { return way1; } }

        public T ReadRaw()
        {
            return way1.Read();
        }

        public T Read()
        {
            return way2.Read();
        }

        public void WriteRaw(T value)
        {
            way1.Write(value);
        }

        public void Write(T value)
        {
            way2.Write(value);
            engine.MarkDirty(this);
        }

        public void CopyCell(int from, int to)
        {
            // Ignore
        }

        public void ClearCell(int at)
        {
            // Ignore
        }

        public static implicit operator T(Tw<T> tw)
        {
            return tw.Read();
        }
    }
}
