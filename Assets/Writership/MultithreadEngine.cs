using System;
using System.Collections.Generic;
using System.Threading;

namespace Writership
{
    public class MultithreadEngine : IEngine, IDisposable
    {
        private class Dirty
        {
            public int Phase;
            public IHaveCells Inner;
        }

        private readonly List<Dirty>[] dirties;
        private readonly Dictionary<object, List<Action>>[] listeners;

        private readonly int mainThreadId;
        private bool isComputeDone;
        private Thread computeThread;
        private AutoResetEvent computeSignal;

        public MultithreadEngine()
        {
            TotalCells = 3;
            dirties = new List<Dirty>[TotalCells];
            listeners = new Dictionary<object, List<Action>>[TotalCells];

            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            isComputeDone = false;
            if (TotalCells == 3)
            {
                computeThread = new Thread(Compute);
                computeThread.Start();
            }
            else
            {
                computeThread = null;
            }
            computeSignal = new AutoResetEvent(false);

            for (int i = 0, n = TotalCells; i < n; ++i)
            {
                dirties[i] = new List<Dirty>();
                listeners[i] = new Dictionary<object, List<Action>>();
            }
        }

        public void Dispose()
        {
            if (computeThread != null)
            {
                computeThread.Abort();
                computeThread = null;
            }
        }

        public int TotalCells { get; private set; }
        public int ReadCellIndex { get { return 0; } }
        public int ComputeCellIndex { get { return WriteCellIndex - 1; } }
        public int WriteCellIndex { get { return TotalCells - 1; } }

        public int CurrentCellIndex
        {
            get
            {
                if (Thread.CurrentThread.ManagedThreadId == mainThreadId) return ReadCellIndex;
                else return ComputeCellIndex;
            }
        }

        public void MarkDirty(IHaveCells target, bool allowMultiple = false)
        {
            var dirties = this.dirties[CurrentCellIndex];
            var dirty = dirties.Find(it => ReferenceEquals(it.Inner, target));
            if (dirty == null)
            {
                dirty = new Dirty
                {
                    Phase = 0,
                    Inner = target,
                };
                dirties.Add(dirty);
            }
            else if (dirty.Phase == 1 && !allowMultiple)
            {
                throw new InvalidOperationException("Cannot mark dirty for same target twice in same run");
            }

            dirty.Phase = 1;
        }

        public IDisposable RegisterListener(object[] targets, Action job)
        {
            RegisterListener(ReadCellIndex, targets, job);
            return new Unregisterer(this, ReadCellIndex, targets, job);
        }

        public IDisposable RegisterComputer(object[] targets, Action job)
        {
            RegisterListener(ComputeCellIndex, targets, job);
            return new Unregisterer(this, ComputeCellIndex, targets, job);
        }

        public void UnregisterListener(int at, object[] targets, Action job)
        {
            // TODO Lock or use thread-safe collections
            // to avoid multiple threads register at same time
            var listeners = this.listeners[at];
            for (int i = 0, n = targets.Length; i < n; ++i)
            {
                var target = targets[i];
                List<Action> jobs;
                if (!listeners.TryGetValue(target, out jobs) || !jobs.Remove(job))
                {
                    throw new InvalidOperationException("Not found on unregister");
                }

                if (jobs.Count <= 0) listeners.Remove(target);
            }
        }

        public void Update()
        {
            while (!isComputeDone)
            {
                Thread.Sleep(1);
            }
            CopyDirties(ComputeCellIndex, ReadCellIndex);
            dirties[ComputeCellIndex].Clear();

            CopyCells(WriteCellIndex, ReadCellIndex);
            CopyDirties(ReadCellIndex, ComputeCellIndex);
            isComputeDone = false;
            computeSignal.Set();

            Notify(ReadCellIndex);
            dirties[ReadCellIndex].Clear();
        }

        private void CopyCells(int from, int to)
        {
            var dirties = this.dirties[to];
            for (int i = 0, n = dirties.Count; i < n; ++i)
            {
                var dirty = dirties[i];
                if (dirty.Phase != 1) continue;
                dirty.Phase = 2;

                dirty.Inner.CopyCell(from, to);
            }
        }

        private bool Notify(int at)
        {
            var dirties = this.dirties[at];
            var listeners = this.listeners[at];
            var calledJobs = new List<Action>();

            for (int i = 0, n = dirties.Count; i < n; ++i)
            {
                var dirty = dirties[i];
                if (dirty.Phase == 2) dirty.Phase = 3;
                else if (dirty.Phase == 11) dirty.Phase = 12;
                else continue;

                List<Action> jobs;
                if (listeners.TryGetValue(dirty.Inner, out jobs))
                {
                    for (int j = 0, m = jobs.Count; j < m; ++j)
                    {
                        var job = jobs[j];
                        if (calledJobs.Contains(job)) continue;
                        calledJobs.Add(job);
                        jobs[j]();
                    }
                }
            }

            for (int i = 0, n = dirties.Count; i < n; ++i)
            {
                var dirty = dirties[i];
                if (dirty.Phase == 3) dirty.Phase = 4;
                else if (dirty.Phase == 12) dirty.Phase = 13;
                else continue;

                dirty.Inner.ClearCell(CurrentCellIndex);
            }

            return calledJobs.Count > 0;
        }

        private void Process(int at)
        {
            bool stillDirty = true;
            while (stillDirty)
            {
                CopyCells(WriteCellIndex, at);
                stillDirty = Notify(at);
            }
        }

        private void Compute()
        {
            while (true)
            {
                Process(1);
                isComputeDone = true;
                computeSignal.WaitOne();
            }
        }

        private void CopyDirties(int from, int to)
        {
            var fromDirties = dirties[from];
            var toDirties = dirties[to];
            for (int i = 0, n = fromDirties.Count; i < n; ++i)
            {
                var dirty = fromDirties[i];
                if (dirty.Phase < 2) throw new NotImplementedException();
                else if (dirty.Phase > 10) continue;

                dirty.Inner.CopyCell(from, to);

                var existing = toDirties.Find(it => ReferenceEquals(it.Inner, dirty.Inner));
                if (existing == null)
                {
                    existing = new Dirty
                    {
                        Phase = 11,
                        Inner = dirty.Inner,
                    };
                    toDirties.Add(existing);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        private void RegisterListener(int at, object[] targets, Action job)
        {
            // TODO Lock or use thread-safe collections
            // to avoid multiple threads register at same time
            var listeners = this.listeners[at];
            for (int i = 0, n = targets.Length; i < n; ++i)
            {
                var target = targets[i];
                List<Action> jobs;
                if (!listeners.TryGetValue(target, out jobs))
                {
                    jobs = new List<Action>();
                    listeners.Add(target, jobs);
                }
                jobs.Add(job);
            }

            if (at == CurrentCellIndex) job();
            else MarkDirty((IHaveCells)targets[0]);
        }
    }
}
