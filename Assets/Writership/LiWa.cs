using System;
using System.Collections.Generic;

namespace Writership
{
    public class LiWa : IOp<Empty>, IHaveCells
    {
        private readonly IOp<Empty> inner;

        public IOp<Empty> Applied { get; private set; }

        public LiWa(IEngine engine)
        {
            inner = engine.Op<Empty>();
            Applied = null;
        }

        public LiWa Setup<T>(CompositeDisposable cd, IEngine engine, ILi<T> li, Func<T, object> extract)
        {
            var lastCd = new CompositeDisposable();
            var lastTargets = new List<object>();
            cd.Add(lastCd);
            engine.Computer(cd,
                new object[] { li },
                () =>
                {
                    var l = li.Read();
                    var targets = new List<object>();
                    for (int i = 0, n = l.Count; i < n; ++i)
                    {
                        targets.Add(extract(l[i]));
                    }

                    if (targets.Count == lastTargets.Count)
                    {
                        bool isSame = true;
                        for (int i = 0, n = targets.Count; i < n; ++i)
                        {
                            if (!ReferenceEquals(targets[i], lastTargets[i]))
                            {
                                isSame = false;
                                break;
                            }
                        }
                        if (isSame) return;
                    }
                    lastTargets.Clear();
                    lastTargets.AddRange(targets);

                    lastCd.Dispose();
                    if (targets.Count > 0)
                    {
                        engine.Computer(lastCd,
                            targets.ToArray(),
                            () =>
                            {
                                // Work around to ensure different instance of action is registerd
                                l.GetHashCode();
                                inner.Fire(Empty.Instance);
                                engine.MarkDirty(this);
                            }
                        );
                    }
                }
            );

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
