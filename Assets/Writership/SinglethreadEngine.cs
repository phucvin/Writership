using System;
using System.Collections.Generic;
using System.Threading;

namespace Writership
{
    public class SinglethreadEngine : IEngine, IDisposable
    {
        private class Dirty
        {
            public int Phase;
            public IHaveCells Inner;
        }

        private readonly int maxWorkerThreads;

        private readonly List<Dirty>[] dirties;
        private readonly Dictionary<object, List<Action>>[] listeners;
        private readonly Dictionary<object, List<Action>>[] pendingRemoveListeners;
        private readonly List<Action>[] pendingListeners;

        public SinglethreadEngine(int maxWorkerThreads = 2)
        {
            this.maxWorkerThreads = maxWorkerThreads;

            dirties = new List<Dirty>[TotalCells];
            listeners = new Dictionary<object, List<Action>>[TotalCells];
            pendingRemoveListeners = new Dictionary<object, List<Action>>[TotalCells];
            pendingListeners = new List<Action>[TotalCells];

            for (int i = 0, n = TotalCells; i < n; ++i)
            {
                dirties[i] = new List<Dirty>();
                listeners[i] = new Dictionary<object, List<Action>>();
                pendingRemoveListeners[i] = new Dictionary<object, List<Action>>();
                pendingListeners[i] = new List<Action>();
            }
        }

        public void Dispose()
        {
        }

        public int TotalCells { get { return 2; } }
        public int MainCellIndex { get { return 0; } }
        public int WorkerCellIndex { get { return 1; } }
        public int WriteCellIndex { get { return 1; } }

        public int CurrentCellIndex
        {
            get
            {
                return MainCellIndex;
            }
        }

        public void MarkDirty(IHaveCells target, bool allowMultiple = false)
        {
            var dirties = this.dirties[CurrentCellIndex];
            lock (dirties) // TODO Better use a thread-safe collection to reduce blocking
            {
                var dirty = dirties.Find(it => ReferenceEquals(it.Inner, target));
                if (dirty == null)
                {
                    dirty = new Dirty
                    {
                        Phase = 1,
                        Inner = target,
                    };
                    dirties.Add(dirty);
                }
                else if (dirty.Phase == 1)
                {
                    if (!allowMultiple) throw new InvalidOperationException("Cannot mark dirty for same target twice in same run");
                }
                else if (dirty.Phase >= 2 && dirty.Phase <= 4)
                {
                    dirty.Phase = 1;
                }
                else throw new NotImplementedException(dirty.Phase.ToString());
            }
        }

        public void Listen(int atCellIndex, CompositeDisposable cd, object[] targets, Action job)
        {
            bool needPending = true;
            if (atCellIndex < 0)
            {
                atCellIndex = -atCellIndex - 1;
                needPending = false;
            }

            // TODO Lock or use thread-safe collections
            // to avoid multiple threads register at same time
            var listeners = this.listeners[atCellIndex];
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

            if (needPending) pendingListeners[atCellIndex].Add(job); // TODO Also need thread-safe here

            if (cd != null) cd.Add(new Unregisterer(this, atCellIndex, targets, job));
        }

        // TODO Rename
        public void UnregisterListener(int at, object[] targets, Action job)
        {
            // TODO Lock or use thread-safe collections
            // to avoid multiple threads register at same time
            var pendingRemoveListeners = this.pendingRemoveListeners[at];
            var listeners = this.listeners[at];
            for (int i = 0, n = targets.Length; i < n; ++i)
            {
                var target = targets[i];
                List<Action> jobs;
                if (!listeners.TryGetValue(target, out jobs) || !jobs.Contains(job))
                {
                    throw new InvalidOperationException("Not found on unregister");
                }
                if (!pendingRemoveListeners.TryGetValue(target, out jobs))
                {
                    jobs = new List<Action>();
                    pendingRemoveListeners.Add(target, jobs);
                }

                jobs.Add(job);
            }
        }

        public void Update()
        {
            Process(MainCellIndex);
            dirties[MainCellIndex].Clear();
        }

        private int CopyCells(int from, int to)
        {
            var dirties = this.dirties[to];
            int copied = 0;
            // TODO Parallel if profiler tell it's slow
            for (int i = 0, n = dirties.Count; i < n; ++i)
            {
                var dirty = dirties[i];
                if (dirty.Phase != 1) continue;
                dirty.Phase = 2;

                dirty.Inner.CopyCell(from, to);
                ++copied;
            }
            return copied;
        }

        // TODO Refactor
        private bool Notify(int at)
        {
            var dirties = this.dirties[at];
            var toCallMainJobs = new List<Action>();
            var toCallWorkerJobs = new List<Action>();

            for (int c = MainCellIndex; c <= WorkerCellIndex; ++c)
            {
                var listeners = this.listeners[c];
                var pendingListeners = this.pendingListeners[c];
                var toCallJobs = c == MainCellIndex ? toCallMainJobs : toCallWorkerJobs;

                // TODO Refactor & move to a method
                var toRemoveKeys = new List<object>();
                foreach (var pair in pendingRemoveListeners[c])
                {
                    List<Action> jobs = listeners[pair.Key];
                    jobs.RemoveAll(it => pair.Value.Contains(it));
                    if (jobs.Count <= 0) toRemoveKeys.Add(pair.Key);
                }
                for (int i = 0, n = toRemoveKeys.Count; i < n; ++i)
                {
                    listeners.Remove(toRemoveKeys[i]);
                }
                pendingRemoveListeners[c].Clear();

                for (int i = 0, n = pendingListeners.Count; i < n; ++i)
                {
                    var job = pendingListeners[i];
                    if (toCallJobs.Contains(job)) continue;
                    toCallJobs.Add(job);
                }
                pendingListeners.Clear();
            }

            for (int i = 0, n = dirties.Count; i < n; ++i)
            {
                var dirty = dirties[i];
                if (dirty.Phase == 2) dirty.Phase = 3;
                else if (dirty.Phase == 11) dirty.Phase = 12;
                else continue;


                for (int c = MainCellIndex; c <= WorkerCellIndex; ++c)
                {
                    var listeners = this.listeners[c];
                    var toCallJobs = c == MainCellIndex ? toCallMainJobs : toCallWorkerJobs;
                    List<Action> jobs;
                    if (listeners.TryGetValue(dirty.Inner, out jobs))
                    {
                        for (int j = 0, m = jobs.Count; j < m; ++j)
                        {
                            var job = jobs[j];
                            if (toCallJobs.Contains(job)) continue;
                            toCallJobs.Add(job);
                        }
                    }
                }
            }

            Parallel(maxWorkerThreads, toCallWorkerJobs, it => it());
            for (int i = 0, n = toCallMainJobs.Count; i < n; ++i)
            {
                toCallMainJobs[i]();
            }

            for (int i = 0, n = dirties.Count; i < n; ++i)
            {
                var dirty = dirties[i];
                if (dirty.Phase == 1) dirty.Phase = 1;
                else if (dirty.Phase == 3) dirty.Phase = 4;
                else if (dirty.Phase == 12) dirty.Phase = 13;
                else continue;

                dirty.Inner.ClearCell(CurrentCellIndex);
            }

            return toCallMainJobs.Count + toCallWorkerJobs.Count > 0;
        }

        private void Process(int at)
        {
            bool stillDirty = true;
            int ran = 0;
            while (stillDirty)
            {
                Notify(at);
                stillDirty = CopyCells(WriteCellIndex, at) > 0 ||
                    pendingListeners[at].Count > 0;
                if (++ran > 1000) throw new StackOverflowException("Engine overflow");
            }
        }

        private static void Parallel<T>(int maxThreads, IList<T> list, Action<T> action)
        {
            int count = list.Count;
            if (count <= 0) return;
            else if (count < maxThreads) maxThreads = count;

            int i = -1;
            var resetEvents = new ManualResetEvent[maxThreads];
            Exception ex = null;

            for (int t = 0; t < maxThreads; ++t)
            {
                var resetEvent = new ManualResetEvent(false);
                resetEvents[t] = resetEvent;
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    UnityEngine.Profiling.Profiler.BeginThreadProfiling("Writership", "Parallel");
                    UnityEngine.Profiling.Profiler.BeginSample("Loop");
                    int j = Interlocked.Increment(ref i);
                    while (j < count)
                    {
                        try
                        {
                            action(list[j]);
                        }
                        catch (Exception e)
                        {
                            ex = e;
                        }
                        j = Interlocked.Increment(ref i);
                    }
                    resetEvent.Set();
                    UnityEngine.Profiling.Profiler.EndSample();
                    UnityEngine.Profiling.Profiler.EndThreadProfiling();
                });
            }

            WaitHandle.WaitAll(resetEvents);
            if (ex != null) throw ex;
        }
    }
}
