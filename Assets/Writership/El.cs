namespace Writership
{
    public class El<T> : IHaveCells
    {
        private readonly MultithreadEngine engine;
        private readonly T[] cells;

#if DEBUG
        private readonly Writership writership = new Writership();
#endif

        public El(MultithreadEngine engine, T value)
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
            writership.Mark();
#endif
            MarkSelfDirty();
            cells[engine.WriteCellIndex] = value;
        }

        public void CopyCell(int from, int to)
        {
            cells[to] = cells[from];
        }

        public void ClearCell(int at) { }

        private void MarkSelfDirty()
        {
            engine.MarkDirty(this);
        }
    }
}
