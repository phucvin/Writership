using System;
using System.Collections.Generic;

namespace Writership
{
    public class Watcher : IOp<Empty>, IHaveCells
    {
        private readonly IOp<Empty> inner;

        public IOp<Empty> Applied { get; private set; }

        public Watcher(IEngine engine)
        {
            inner = engine.Op<Empty>();
            Applied = null;
        }

        public Watcher Setup<T>(CompositeDisposable cd, IEngine engine, ILi<T> li, Func<T, object> extract)
        {
            IDisposable last = null;
            cd.Add(engine.RegisterComputer(
                new object[] { li },
                () =>
                {
                    var l = li.Read();
                    var targets = new List<object>();
                    for (int i = 0, n = l.Count; i < n; ++i)
                    {
                        targets.Add(extract(l[i]));
                    }
                    if (last != null)
                    {
                        last.Dispose();
                        last = null;
                    }
                    if (targets.Count > 0)
                    {
                        last = engine.RegisterComputer(
                            targets.ToArray(),
                            () =>
                            {
                                inner.Fire(Empty.Instance);
                                engine.MarkDirty(this, allowMultiple: true);
                            }
                        );
                    }
                }
            ));

            return this;
        }

        public void Fire(Empty value)
        {
            throw new NotSupportedException();
        }

        public IList<Empty> Read()
        {
            return inner.Read();
        }

        public void CopyCell(int from, int to)
        {
            // Ignore
        }

        public void ClearCell(int at)
        {
            // Ingore
        }
    }
}
