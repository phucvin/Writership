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
    public class Op<T> : IOp<T>, IHaveCells
    {
        // TODO Maybe not a good solution, should compute by changed values (of dynamic children)
        private readonly Op<Empty> applied;

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
                applied = new Op<Empty>(engine, needApplied: false);

                engine.RegisterComputer(new object[] { this }, () =>
                {
                    if (Read().Count > 0)
                    {
                        applied.Fire(default(Empty));
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
