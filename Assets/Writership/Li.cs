using System.Collections.Generic;

namespace Writership
{
    public interface ILi<T>
    {
        IList<T> Read();
        List<T> AsWrite();
    }

    public class Li<T> : ILi<T>, IHaveCells
    {
        private readonly IEngine engine;
        private readonly List<T>[] cells;

#if DEBUG
        private readonly Writership writership = new Writership();
#endif

        public Li(IEngine engine, IList<T> list)
        {
            this.engine = engine;

            cells = new List<T>[engine.TotalCells];
            for (int i = 0, n = cells.Length; i < n; ++i)
            {
                var l = new List<T>();
                l.AddRange(list);
                cells[i] = l;
            }
        }

        public IList<T> Read()
        {
            return cells[engine.CurrentCellIndex].AsReadOnly();
        }

        public List<T> AsWrite()
        {
#if DEBUG
            writership.Mark();
#endif
            MarkSelfDirty();
            return cells[engine.WriteCellIndex];
        }

        public void CopyCell(int from, int to)
        {
            cells[to].Clear();
            cells[to].AddRange(cells[from]);
        }

        public void ClearCell(int at) { }

        private void MarkSelfDirty()
        {
            engine.MarkDirty(this);
        }
    }
}
