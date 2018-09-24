using System;

namespace Writership
{
    public interface IEl<T> : IReadableWriteable<T>
    {
        [Obsolete]
        bool IsChanged { get; }
    }

    public class El<T> : IEl<T>, IHaveCells
    {
        private readonly IEngine engine;
        private readonly T[] cells;
        private readonly bool[] isChangeds;

#if DEBUG
        private readonly Writership writership = new Writership();
#endif

        public El(IEngine engine, T value)
        {
            this.engine = engine;

            cells = new T[engine.TotalCells];
            isChangeds = new bool[engine.TotalCells - 1];
            for (int i = 0, n = cells.Length; i < n; ++i)
            {
                cells[i] = value;
                if (i < n - 1) isChangeds[i] = false;
            }
        }

        [Obsolete]
        public bool IsChanged { get { return isChangeds[engine.CurrentCellIndex]; } }

        public T Read()
        {
            return cells[engine.CurrentCellIndex];
        }

        public void Write(T value)
        {
#if DEBUG
            writership.Mark();
#endif
            if (Equals(value, Read())) return;
            MarkSelfDirty();
            cells[engine.WriteCellIndex] = value;
        }

        public void CopyCell(int from, int to)
        {
            cells[to] = cells[from];
            isChangeds[to] = true;
        }

        public void ClearCell(int at)
        {
            isChangeds[at] = false;
        }

        private void MarkSelfDirty()
        {
            engine.MarkDirty(this);
        }

        public override string ToString()
        {
            return Read().ToString();
        }

        public static implicit operator T(El<T> el)
        {
            return el.Read();
        }
    }
}
