using System;
using System.Collections.Generic;

namespace Writership
{
    public class SinglethreadEngine : IEngine, IDisposable
    {
        private class Dirty
        {
            public int Phase;
            public IHaveCells Inner;
        }

        private readonly List<Dirty>[] dirties;
        private readonly Dictionary<object, List<Action>>[] listeners;

        public SinglethreadEngine()
        {
            TotalCells = 2;

            dirties = new List<Dirty>[TotalCells];
            listeners = new Dictionary<object, List<Action>>[TotalCells];

            for (int i = 0, n = TotalCells; i < n; ++i)
            {
                dirties[i] = new List<Dirty>();
                listeners[i] = new Dictionary<object, List<Action>>();
            }
        }

        public void Dispose()
        {
        }

        public int TotalCells { get; private set; }
        public int ReadCellIndex { get { return 0; } }
        public int ComputeCellIndex { get { return WriteCellIndex - 1; } }
        public int WriteCellIndex { get { return TotalCells - 1; } }

        public int CurrentCellIndex
        {
            get
            {
                return ReadCellIndex;
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
            return new Unregisterer(this, ReadCellIndex, targets, job);
        }

        public void UnregisterListener(int at, object[] targets, Action job)
        {
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
            Process(ReadCellIndex);
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
            int ran = 0;
            while (stillDirty)
            {
                CopyCells(WriteCellIndex, at);
                stillDirty = Notify(at);
                if (++ran > 1000) throw new StackOverflowException("Engine overflow");
            }
        }

        private void RegisterListener(int at, object[] targets, Action job)
        {
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

            job();
        }
    }
}
