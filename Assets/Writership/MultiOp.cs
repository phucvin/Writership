﻿using System;
using System.Collections.Generic;

namespace Writership
{
    public interface IMultiOp<T> : IFireable<T>
    {
        IList<T> Read();
    }

    public class MultiOp<T> : IMultiOp<T>, IHaveCells
    {
        private class WithFlag
        {
            public int Flag;
            public T Value;
        }

        // TODO Remove because maybe not a good solution,
        // should compute by changed values (of dynamic children), use Wa instead
        private readonly MultiOp<Empty> applied;

        private readonly IEngine engine;
        private readonly bool allowWriters;
        private readonly Func<T, T, T> reducer;
        private readonly List<T>[] cells;
        private readonly IList<T>[] readonlyCells;
        private readonly T[] reducedCells;
        private readonly List<WithFlag> writeCell;

#if DEBUG
        private readonly Writership writership = new Writership();
#endif

        private int lastCellIndex;

        public MultiOp(IEngine engine, bool allowWriters = false,
            Func<T, T, T> reducer = null)
        {
            this.engine = engine;
            this.allowWriters = allowWriters;
            this.reducer = reducer;

            cells = new List<T>[engine.TotalCells - 1];
            readonlyCells = new IList<T>[engine.TotalCells - 1];
            reducedCells = reducer != null ? new T[engine.TotalCells - 1] : null;
            for (int i = 0, n = cells.Length; i < n; ++i)
            {
                var l = new List<T>();
                cells[i] = l;
                readonlyCells[i] = l.AsReadOnly();
                if (reducedCells != null) reducedCells[i] = default(T);
            }
            writeCell = new List<WithFlag>();

            lastCellIndex = allowWriters ? engine.MainCellIndex : -1;
        }

        public int Count { get { return Read().Count; } }
        public T First { get { return this[0]; } }
        public T Last { get { return this[Count - 1]; } }
        public T Reduced { get { return reducedCells[engine.CurrentCellIndex]; } }

        public T this[int i]
        {
            get { return Read()[i]; }
        }

        public IList<T> Read()
        {
            return readonlyCells[engine.CurrentCellIndex];
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
            writeCell.Add(new WithFlag { Flag = 0, Value = value });
        }

        public void CopyCell(int from, int to)
        {
            if (from == engine.WriteCellIndex)
            {
                var toCell = cells[to];
                toCell.Clear();
                if (reducedCells != null) reducedCells[to] = default(T);

                for (int i = 0, n = writeCell.Count; i < n; ++i)
                {
                    var withFlag = writeCell[i];
                    if (withFlag.Flag == 0 || withFlag.Flag == (engine.TotalCells - (to + 1)))
                    {
                        withFlag.Flag += (to + 1);
                        toCell.Add(withFlag.Value);
                        if (reducedCells != null) reducedCells[to] = reducer(reducedCells[to], withFlag.Value);
                    }
                }

                if (engine.TotalCells > 2) writeCell.RemoveAll(it => it.Flag == engine.TotalCells);
                else writeCell.RemoveAll(it => it.Flag > 0);
            }
            else
            {
                cells[to].Clear();
                cells[to].AddRange(cells[from]);
                if (reducedCells != null) reducedCells[to] = reducedCells[from];
            }
        }

        public void ClearCell(int at)
        {
            cells[at].Clear();
            if (reducedCells != null) reducedCells[at] = default(T);
        }

        private void MarkSelfDirty()
        {
            engine.MarkDirty(this, allowMultiple: true);
        }

        public static implicit operator bool(MultiOp<T> op)
        {
            return op.Read().Count > 0;
        }
    }
}
