using System;

namespace Writership
{
    public interface IOp<T> : IFireable<T>
    {
        bool Has();
        bool TryRead(out T value);
    }

    public class Op<T> : IOp<T>, IHaveCells
    {
        private class Cell
        {
            public bool Has;
            public T Value;
        }

        private readonly IEngine engine;
        private readonly bool allowWriters;
        private readonly Cell[] cells;

#if DEBUG
        private readonly Writership writership = new Writership();
#endif

        private int lastCellIndex;

        public Op(IEngine engine, bool allowWriters = false)
        {
            this.engine = engine;
            this.allowWriters = allowWriters;

            cells = new Cell[engine.TotalCells];
            for (int i = 0, n = cells.Length; i < n; ++i)
            {
                cells[i] = new Cell { Has = false, Value = default(T) };
            }

            lastCellIndex = allowWriters ? engine.MainCellIndex : -1;
        }

        public bool Has()
        {
            return cells[engine.CurrentCellIndex].Has;
        }

        public bool TryRead(out T value)
        {
            var cell = cells[engine.CurrentCellIndex];
            if (cell.Has) value = cell.Value;
            else value = default(T);
            return cell.Has;
        }

        public void Fire(T value)
        {
#if DEBUG
            if (!allowWriters) writership.Mark();
#endif
            if (lastCellIndex >= 0 && engine.CurrentCellIndex != lastCellIndex)
            {
                throw new InvalidOperationException("Op.Fire must be in same thread");
            }
            lastCellIndex = engine.CurrentCellIndex;
            MarkSelfDirty();
            var cell = cells[engine.WriteCellIndex];
            cell.Has = true;
            cell.Value = value;
        }

        public void CopyCell(int from, int to)
        {
            var fromCell = cells[from];
            var toCell = cells[to];
            toCell.Has = fromCell.Has;
            toCell.Value = fromCell.Value;
        }

        public void ClearCell(int at)
        {
            var cell = cells[at];
            cell.Has = false;
            cell.Value = default(T);
        }

        private void MarkSelfDirty()
        {
            engine.MarkDirty(this);
        }

        public static implicit operator bool(Op<T> op)
        {
            return op.Has();
        }

        public static implicit operator T(Op<T> op)
        {
            T value;
            if (op.TryRead(out value)) return value;
            else throw new InvalidOperationException();
        }
    }
}
