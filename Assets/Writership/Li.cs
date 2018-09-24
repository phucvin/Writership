using System.Collections.Generic;

namespace Writership
{
    public interface ILi<T> : IReadable<IList<T>>
    {
        List<T> AsWrite();
    }

    public class Li<T> : ILi<T>, IHaveCells
    {
        private readonly IEngine engine;
        private readonly List<T>[] cells;
        private readonly IList<T>[] readonlyCells;

#if DEBUG
        private readonly Writership writership = new Writership();
#endif

        public Li(IEngine engine, IList<T> list)
        {
            this.engine = engine;

            cells = new List<T>[engine.TotalCells];
            readonlyCells = new IList<T>[engine.TotalCells - 1];
            for (int i = 0, n = cells.Length; i < n; ++i)
            {
                var l = new List<T>();
                l.AddRange(list);
                cells[i] = l;
                if (i < n - 1) readonlyCells[i] = l.AsReadOnly();
            }
        }

        public int Count { get { return Read().Count; } }

        public T this[int i]
        {
            get { return Read()[i]; }
        }

        public IList<T> Read()
        {
            return readonlyCells[engine.CurrentCellIndex];
        }

        public List<T> AsWrite()
        {
#if DEBUG
            writership.Mark();
#endif
            MarkSelfDirty();
            return cells[engine.WriteCellIndex];
        }

        // TODO Improve by caching, or not creating temporary array unless dirty
        public WriteProxy AsWriteProxy()
        {
            return new WriteProxy(this);
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

        public class WriteProxy : List<T>
        {
            private readonly Li<T> li;

            public WriteProxy(Li<T> li)
                : base(li.Read())
            {
                this.li = li;
            }

            public bool Commit()
            {
                bool isDirty = false;
                var org = li.Read();
                if (Count != org.Count) isDirty = true;
                else
                {
                    for (int i = 0, n = Count; i < n; ++i)
                    {
                        if (!Equals(this[i], org[i]))
                        {
                            isDirty = true;
                            break;
                        }
                    }
                }

                if (isDirty)
                {
                    var write = li.AsWrite();
                    write.Clear();
                    write.AddRange(this);
                }
                return isDirty;
            }
        }
    }
}
