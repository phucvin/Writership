using System;
using System.Collections.Generic;

namespace Writership
{
    public interface IWa : IReadable<int>
    {
    }

    public class Wa : IWa, IHaveCells
    {
        private readonly MultiOp<Empty> inner;

        public Wa(IEngine engine)
        {
            inner = engine.MultiOp<Empty>();
        }

        internal void Setup<T>(CompositeDisposable cd, IEngine engine, ILi<T> li, Func<T, object> extractor)
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
                        targets.Add(extractor(l[i]));
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
        }

        public int Read()
        {
            return inner.Read().Count;
        }

        public void CopyCell(int from, int to)
        {
            // Ignore
        }

        public void ClearCell(int at)
        {
            // Ingore
        }

        public static implicit operator bool(Wa wa)
        {
            return wa.Read() > 0;
        }
    }
}
