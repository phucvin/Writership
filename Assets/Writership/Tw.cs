namespace Writership
{
    public interface ITw<T> : IReadable<T>, IWriteable<T>
    {
    }

    public class Tw<T> : ITw<T>, IHaveCells
    {
        private readonly IEngine engine;
        private readonly T[] cells;

#if DEBUG
        private readonly Writership writership1 = new Writership();
        private readonly Writership writership2 = new Writership();
#endif

        public Tw(IEngine engine, T value)
        {
            this.engine = engine;

            cells = new T[engine.TotalCells];
            for (int i = 0, n = cells.Length; i < n; ++i)
            {
                cells[i] = value;
            }
        }

        public T Read()
        {
            return cells[engine.CurrentCellIndex];
        }

        public void Write(T value)
        {
#if DEBUG
            if (!writership1.TryMark())
            {
                writership2.Mark();
            }
#endif
            if (Equals(value, Read())) return;
            MarkSelfDirty();
            cells[engine.WriteCellIndex] = value;
        }

        public void CopyCell(int from, int to)
        {
            cells[to] = cells[from];
        }

        public void ClearCell(int at)
        {
        }

        private void MarkSelfDirty()
        {
            engine.MarkDirty(this);
        }

        public override string ToString()
        {
            return Read().ToString();
        }

        public static implicit operator T(Tw<T> el)
        {
            return el.Read();
        }
    }
}
