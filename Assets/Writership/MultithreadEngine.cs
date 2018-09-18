using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Dictionary<object, List<Action>>[] pendingRemoveListeners;
        private readonly List<Action>[] pendingListeners;

        private readonly int mainThreadId;
        private bool isComputeDone;
        private Exception computeException;
        private WaitCallback computeWorkItem;

        public MultithreadEngine()
        {
            TotalCells = 3;
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

            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            // TODO Maybe not need, but should find a way to make sure CurrentCellIndex is correct in any case
            if (mainThreadId != 1) throw new InvalidOperationException("MultithreadEngine can only be created at main thread");
            isComputeDone = false;
            computeException = null;
            computeWorkItem = new WaitCallback(Compute);
            ThreadPool.QueueUserWorkItem(computeWorkItem);
        }

        public void Dispose()
        {
        }

        public int TotalCells { get; private set; }
        public int MainCellIndex { get { return 0; } }
        public int WorkerCellIndex { get { return 1; } }
        public int WriteCellIndex { get { return 2; } }

        public int CurrentCellIndex
        {
            get
            {
                if (Thread.CurrentThread.ManagedThreadId == mainThreadId) return MainCellIndex;
                else return WorkerCellIndex;
            }
        }

        public void MarkDirty(IHaveCells target, bool allowMultiple = false)
        {
            // TODO Lock or use thread-safe collections
            // Because notify can be parallel
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

            pendingListeners[atCellIndex].Add(job); // TODO Also need thread-safe here

            if (cd != null) cd.Add(new Unregisterer(this, atCellIndex, targets, job));
        }

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

        // Should call after all writes done in a frame
        public void Update()
        {
            while (!isComputeDone)
            {
                Thread.Sleep(1);
                if (computeException != null)
                {
                    throw computeException;
                }
            }
            CopyDirties(WorkerCellIndex, MainCellIndex);
            dirties[WorkerCellIndex].Clear();

            Process(MainCellIndex);

            CopyDirties(MainCellIndex, WorkerCellIndex);
            // TODO Skip compute if not dirty
            isComputeDone = false;
            ThreadPool.QueueUserWorkItem(computeWorkItem);

            dirties[MainCellIndex].Clear();
        }

        private int CopyCells(int from, int to)
        {
            var dirties = this.dirties[to];
            int copied = 0;
            // TODO Parallel
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

        private int Notify(int at)
        {
            var dirties = this.dirties[at];
            var listeners = this.listeners[at];
            var pendingListeners = this.pendingListeners[at];
            var calledJobs = new List<Action>();

            // TODO Refactor & move to a method
            var toRemoveKeys = new List<object>();
            foreach (var pair in pendingRemoveListeners[at])
            {
                List<Action> jobs = listeners[pair.Key];
                jobs.RemoveAll(it => pair.Value.Contains(it));
                if (jobs.Count <= 0) toRemoveKeys.Add(pair.Key);
            }
            for (int i = 0, n = toRemoveKeys.Count; i < n; ++i)
            {
                listeners.Remove(toRemoveKeys[i]);
            }
            pendingRemoveListeners[at].Clear();

            for (int i = 0, n = pendingListeners.Count; i < n; ++i)
            {
                var job = pendingListeners[i];
                if (calledJobs.Contains(job)) continue;
                calledJobs.Add(job);
                //job();
            }
            pendingListeners.Clear();

            // TODO Parallel notify, but have to use thread-safe collections for calledJobs
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
                        //job();
                    }
                }
            }

            if (at == WorkerCellIndex) Parallel(calledJobs, it => it());
            else
            {
                for (int i = 0, n = calledJobs.Count; i < n; ++i)
                {
                    calledJobs[i]();
                }
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

            return calledJobs.Count;
        }

        private void Process(int at)
        {
            bool stillDirty = true;
            int ran = 0;
            while (stillDirty)
            {
                UnityEngine.Profiling.Profiler.BeginSample("Notify");
                Notify(at);
                UnityEngine.Profiling.Profiler.EndSample();
                UnityEngine.Profiling.Profiler.BeginSample("CopyCells");
                stillDirty = CopyCells(WriteCellIndex, at) > 0 ||
                    pendingListeners[at].Count > 0;
                UnityEngine.Profiling.Profiler.EndSample();
                if (++ran > 1000) throw new StackOverflowException("Engine overflow");
            }
        }

        private void Compute(object _)
        {
            UnityEngine.Profiling.Profiler.BeginThreadProfiling("Writership", "Compute");
            try
            {
                UnityEngine.Profiling.Profiler.BeginSample("Process");
                Process(1);
                UnityEngine.Profiling.Profiler.EndSample();
                isComputeDone = true;
            }
            catch (Exception e)
            {
                computeException = e;
            }
            UnityEngine.Profiling.Profiler.EndThreadProfiling();
        }

        private void CopyDirties(int from, int to)
        {
            var fromDirties = dirties[from];
            var toDirties = dirties[to];
            // TODO Parallel
            for (int i = 0, n = fromDirties.Count; i < n; ++i)
            {
                var dirty = fromDirties[i];
                if (dirty.Phase < 2) throw new NotImplementedException();
                else if (dirty.Phase > 10) continue;

                dirty.Inner.CopyCell(WriteCellIndex, to);

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

        public static void Parallel<T>(IEnumerable<T> list, Action<T> action)
        {
            var count = list.Count();

            if (count == 0)
            {
                return;
            }
            else
            {
                // Launch each method in it's own thread
                const int MaxHandles = 64;
                for (var offset = 0; offset <= count / MaxHandles; offset++)
                {
                    // break up the list into 64-item chunks because of a limitiation in WaitHandle
                    var chunk = list.Skip(offset * MaxHandles).Take(MaxHandles);

                    // Initialize the reset events to keep track of completed threads
                    var resetEvents = new ManualResetEvent[chunk.Count()];

                    // spawn a thread for each item in the chunk
                    int i = 0;
                    Exception ex = null;
                    foreach (var item in chunk)
                    {
                        resetEvents[i] = new ManualResetEvent(false);
                        ThreadPool.QueueUserWorkItem(new WaitCallback((object data) =>
                        {
                            UnityEngine.Profiling.Profiler.BeginThreadProfiling("Writership", "Compute");
                            UnityEngine.Profiling.Profiler.BeginSample("Notify");
                            int methodIndex = (int)((object[])data)[0];

                            // Execute the method and pass in the enumerated item
                            try
                            {
                                action((T)((object[])data)[1]);
                            }
                            catch (Exception e)
                            {
                                ex = e;
                            }
                            finally
                            {
                                // Tell the calling thread that we're done
                                resetEvents[methodIndex].Set();
                                UnityEngine.Profiling.Profiler.EndSample();
                                UnityEngine.Profiling.Profiler.EndThreadProfiling();
                            }
                        }), new object[] { i, item });
                        i++;
                    }

                    // Wait for all threads to execute
                    WaitHandle.WaitAll(resetEvents);
                    if (ex != null) throw ex;
                }
            }
        }
    }
}
