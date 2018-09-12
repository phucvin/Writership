﻿using System;
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
        private readonly List<T>[] cells;
        private readonly List<WithFlag> writeCell;

        public Op(IEngine engine, bool needApplied = true)
        {
            this.engine = engine;

            cells = new List<T>[engine.TotalCells];
            for (int i = 0, n = cells.Length; i < n; ++i)
            {
                var l = new List<T>();
                cells[i] = l;
            }
            writeCell = new List<WithFlag>();

            if (needApplied)
            {
                applied = new Op<Empty>(engine, needApplied: false);

                engine.RegisterComputer(new object[] { this }, () =>
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
            if (engine.TotalCells <= 2) writeCell.Clear();
        }

        private void MarkSelfDirty()
        {
            engine.MarkDirty(this, allowMultiple: true);
        }
    }
}
