using System.Collections.Generic;

namespace Writership
{
    public class Op<T> : IHaveCells
    {
        private readonly MultithreadEngine engine;
        private readonly List<T>[] cells;

        public Op(MultithreadEngine engine)
        {
            this.engine = engine;

            cells = new List<T>[engine.TotalCells];
            for (int i = 0, n = cells.Length; i < n; ++i)
            {
                var l = new List<T>();
                cells[i] = l;
            }
        }

        public IList<T> Read()
        {
            return cells[engine.CurrentCellIndex].AsReadOnly();
        }

        public void Fire(T value)
        {
            MarkSelfDirty();
            cells[engine.WriteCellIndex].Add(value);
        }

        public void CopyCell(int from, int to)
        {
            cells[to].Clear();
            cells[to].AddRange(cells[from]);
        }

        public void ClearCell(int at)
        {
            cells[at].Clear();
            cells[engine.WriteCellIndex].Clear();
        }

        private void MarkSelfDirty()
        {
            engine.MarkDirty(this, allowMultiple: true);
        }
    }
}
