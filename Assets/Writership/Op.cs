﻿using System.Collections.Generic;

namespace Writership
{
    public class Op<T> : IHaveCells
    {
        public readonly Op<T> Applied;

        private readonly IEngine engine;
        private readonly List<T>[] cells;

        public Op(IEngine engine, bool needApplied = true)
        {
            this.engine = engine;

            cells = new List<T>[engine.TotalCells];
            for (int i = 0, n = cells.Length; i < n; ++i)
            {
                var l = new List<T>();
                cells[i] = l;
            }

            if (needApplied)
            {
                Applied = new Op<T>(engine, needApplied: false);

                engine.RegisterComputer(new object[] { this }, () =>
                {
                    var self = Read();
                    for (int i = 0, n = self.Count; i < n; ++i)
                    {
                        Applied.Fire(self[i]);
                    }
                });
            }
            else
            {
                Applied = null;
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
