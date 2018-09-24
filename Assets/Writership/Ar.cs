using System;

namespace Writership
{
    public interface IAr<T> : IReadable<T[]>
    {
        T[] AsWrite();
    }

    public class Ar<T> : IAr<T>, IHaveCells
        where T : struct
    {
        private readonly IEngine engine;
        private readonly T[][] cells;

#if DEBUG
        private readonly Writership writership = new Writership();
#endif

        public Ar(IEngine engine, T[] array)
        {
            this.engine = engine;

            cells = new T[engine.TotalCells][];
            for (int i = 0, n = cells.Length; i < n; ++i)
            {
                if (i == 0) cells[i] = array;
                else
                {
                    cells[i] = new T[array.Length];
                    Array.Copy(array, cells[i], array.Length);
                }
            }
        }

        public T[] Read()
        {
            return cells[engine.CurrentCellIndex];
        }

        public T[] AsWrite()
        {
#if DEBUG
            writership.Mark();
#endif
            MarkSelfDirty();
            return cells[engine.WriteCellIndex];
        }

        public void CopyCell(int from, int to)
        {
            Array.Copy(cells[from], cells[to], cells[from].Length);
        }

        public void ClearCell(int at) { }

        private void MarkSelfDirty()
        {
            engine.MarkDirty(this);
        }
    }
}
