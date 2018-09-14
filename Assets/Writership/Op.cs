using System;
using System.Collections.Generic;

namespace Writership
{
    public interface IOp<T>
    {
        IList<T> Read();
        void Fire(T value);
        IOp<Empty> Applied { get; }
    }

    internal class Op<T> : IOp<T>, IHaveCells
    {
        private class WithFlag
        {
            public int Flag;
            public T Value;
        }

        // TODO Maybe not a good solution, should compute by changed values (of dynamic children)
        private readonly Op<Empty> applied;

        private readonly IEngine engine;
        private readonly bool allowWriters;
        private readonly List<T>[] cells;
        private readonly List<WithFlag> writeCell;

#if DEBUG
        private readonly Writership writership = new Writership();
#endif

        private int lastCellIndex;

        public Op(IEngine engine, bool allowWriters = false, bool needApplied = false)
        {
            this.engine = engine;
            this.allowWriters = allowWriters;

            cells = new List<T>[engine.TotalCells];
            for (int i = 0, n = cells.Length; i < n; ++i)
            {
                var l = new List<T>();
                cells[i] = l;
            }
            writeCell = new List<WithFlag>();

            if (needApplied)
            {
                applied = new Op<Empty>(engine, allowWriters: false, needApplied: false);

                engine.Computer(null, new object[] { this }, () =>
                {
                    if (Read().Count > 0)
                    {
                        applied.Fire(Empty.Instance);
                    }
                });
            }
            else
            {
                applied = null;
            }

            lastCellIndex = allowWriters ? engine.MainCellIndex : -1;
        }

        public IOp<Empty> Applied
        {
            get
            {
                if (applied == null) throw new InvalidOperationException("No Applied on create");
                else return applied;
            }
        }

        public IList<T> Read()
        {
            return cells[engine.CurrentCellIndex].AsReadOnly();
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

                for (int i = 0, n = writeCell.Count; i < n; ++i)
                {
                    var withFlag = writeCell[i];
                    if (withFlag.Flag == 0 || withFlag.Flag == (engine.TotalCells - (to + 1)))
                    {
                        withFlag.Flag += (to + 1);
                        toCell.Add(withFlag.Value);
                    }
                }
                if (engine.TotalCells > 2) writeCell.RemoveAll(it => it.Flag == engine.TotalCells);
                else writeCell.RemoveAll(it => it.Flag > 0);
            }
            else
            {
                cells[to].Clear();
                cells[to].AddRange(cells[from]);
            }
        }

        public void ClearCell(int at)
        {
            cells[at].Clear();
        }

        private void MarkSelfDirty()
        {
            engine.MarkDirty(this, allowMultiple: true);
        }
    }
}
