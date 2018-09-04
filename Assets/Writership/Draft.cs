using System;
using System.Collections.Generic;
using System.Threading;

namespace Writership
{
    // TODO Create interface IEngine
    // And implement Engine and DualEngine separately
    // (no need preprocessor WRITERSHIP_NO_COMPUTE_THREAD)
    public class Engine : IDisposable
    {
        private class Dirty
        {
            public int Phase;
            public IHaveCells Inner;
        }

        private readonly List<Dirty>[] dirties;
        private readonly Dictionary<object, List<Action>>[] listeners;

#if !WRITERSHIP_NO_COMPUTE_THREAD
        private readonly int mainThreadId;
        private bool isComputeDone;
        private Thread computeThread;
        private AutoResetEvent computeSignal;
#endif

        public Engine()
        {
#if !WRITERSHIP_NO_COMPUTE_THREAD
            TotalCells = 3;
#else
            TotalCells = 2;
#endif
            dirties = new List<Dirty>[TotalCells];
            listeners = new Dictionary<object, List<Action>>[TotalCells];

#if !WRITERSHIP_NO_COMPUTE_THREAD
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
#endif

            for (int i = 0, n = TotalCells; i < n; ++i)
            {
                dirties[i] = new List<Dirty>();
                listeners[i] = new Dictionary<object, List<Action>>();
            }
        }

        public void Dispose()
        {
#if !WRITERSHIP_NO_COMPUTE_THREAD
            if (computeThread != null)
            {
                computeThread.Abort();
                computeThread = null;
            }
#endif
        }

        public int TotalCells { get; private set; }
        public int WriteCellIndex { get { return TotalCells - 1; } }

        public int CurrentCellIndex
        {
            get
            {
#if !WRITERSHIP_NO_COMPUTE_THREAD
                if (Thread.CurrentThread.ManagedThreadId == mainThreadId) return 0;
                else return 1;
#else
                return 0;
#endif
            }
        }

        public void MarkDirty(IHaveCells target)
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
            else if (dirty.Phase == 1)
            {
                throw new InvalidOperationException("Cannot mark dirty for same target twice in same run");
            }

            dirty.Phase = 1;
        }

        public void RegisterListener(object[] targets, Action job)
        {
            // TODO Lock to avoid multiple threads register at same time
            var listeners = this.listeners[0];
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
            // TODO Return disposable to unregister
        }

        public void RegisterComputer(object[] targets, Action job)
        {
            // TODO Reduce duplication with RegisterListener
            // TODO Lock to avoid multiple threads register at same time
            var listeners = this.listeners[WriteCellIndex - 1];
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
            // TODO Return disposable to unregister
        }

        public void Update()
        {
#if !WRITERSHIP_NO_COMPUTE_THREAD
            while (!isComputeDone)
            {
                Thread.Sleep(1);
            }
            CopyDirties(1, 0);
            dirties[1].Clear();

            Process(0);
            CopyDirties(0, 1);
            dirties[0].Clear();

            isComputeDone = false;
            computeSignal.Set();
#else
            Process(0);
            dirties[0].Clear();
#endif
        }

        private void Process(int at)
        {
            var dirties = this.dirties[at];
            var listeners = this.listeners[at];
            var calledJobs = new List<Action>();
            int ran = 0;

            do
            {
                ++ran;
                calledJobs.Clear();

                for (int i = 0, n = dirties.Count; i < n; ++i)
                {
                    var dirty = dirties[i];
                    if (dirty.Phase != 1) continue;
                    dirty.Phase = 2;

                    dirty.Inner.CopyCells(WriteCellIndex, at);
                }

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

                // FIXME Clear too soon, or copy to compute too late
                // TODO Clear from write cell too
                for (int i = 0, n = dirties.Count; i < n; ++i)
                {
                    var dirty = dirties[i];
                    if (dirty.Phase == 3) dirty.Phase = 4;
                    else if (dirty.Phase == 12) dirty.Phase = 13;
                    else continue;

                    dirty.Inner.ClearCell(CurrentCellIndex);
                }
            }
            while (calledJobs.Count > 0);
        }

#if !WRITERSHIP_NO_COMPUTE_THREAD
        private void Compute()
        {
            try {
                while (true)
                {
                    Process(1);
                    isComputeDone = true;
                    computeSignal.WaitOne();
                }
            }
            catch (Exception e) { UnityEngine.Debug.LogError(e); }
        }

        private void CopyDirties(int from, int to)
        {
            var fromDirties = dirties[from];
            var toDirties = dirties[to];
            for (int i = 0, n = fromDirties.Count; i < n; ++i)
            {
                var dirty = fromDirties[i];
                if (dirty.Phase < 4) throw new NotImplementedException();
                else if (dirty.Phase > 10) continue;

                dirty.Inner.CopyCells(from, to);

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
#endif
    }

    public static class CreateExtensions
    {
        public static El<T> El<T>(this Engine engine, T value)
        {
            return new El<T>(engine, value);
        }

        public static Li<T> Li<T>(this Engine engine, IList<T> list)
        {
            return new Li<T>(engine, list);
        }

        public static Op<T> Op<T>(this Engine engine)
        {
            return new Op<T>(engine);
        }
    }

    public interface IHaveCells
    {
        void CopyCells(int from, int to);
        void ClearCell(int at);
    }

    public class Writership
    {
        private string fileName;
        private int lineNumber;

        public void Mark()
        {
            var frame = new System.Diagnostics.StackFrame(2, true);
            string nowFileName = frame.GetFileName();
            int nowLineNumber = frame.GetFileLineNumber();

            if (fileName == null)
            {
                fileName = nowFileName;
                lineNumber = nowLineNumber;
            }
            else if (fileName != nowFileName || lineNumber != nowLineNumber)
            {
                UnityEngine.Debug.Log("Now write: " + nowFileName + ":" + nowLineNumber);
                UnityEngine.Debug.Log("Org write: " + fileName + ":" + lineNumber);
                throw new InvalidOperationException("Cannot write to same at different places");
            }
        }
    }

    public class El<T> : IHaveCells
    {
        private readonly Engine engine;
        private readonly T[] cells;

#if DEBUG
        private readonly Writership writership = new Writership();
#endif

        public El(Engine engine, T value)
        {
            this.engine = engine;

            cells = new T[engine.TotalCells];
            for (int i = 0, n = cells.Length; i < n; ++i)
            {
                cells[i] = value;
            }
        }

        public T Read()
        {
            return cells[engine.CurrentCellIndex];
        }

        public void Write(T value)
        {
#if DEBUG
            writership.Mark();
#endif
            MarkSelfDirty();
            cells[engine.WriteCellIndex] = value;
        }

        public void CopyCells(int from, int to)
        {
            cells[to] = cells[from];
        }

        public void ClearCell(int at) { }

        private void MarkSelfDirty()
        {
            engine.MarkDirty(this);
        }
    }

    public class Li<T> : IHaveCells
    {
        private readonly Engine engine;
        private readonly List<T>[] cells;

#if DEBUG
        private readonly Writership writership = new Writership();
#endif

        public Li(Engine engine, IList<T> list)
        {
            this.engine = engine;

            cells = new List<T>[engine.TotalCells];
            for (int i = 0, n = cells.Length; i < n; ++i)
            {
                var l = new List<T>();
                l.AddRange(list);
                cells[i] = l;
            }
        }

        public IList<T> Read()
        {
            return cells[engine.CurrentCellIndex].AsReadOnly();
        }

        public List<T> AsWrite()
        {
#if DEBUG
            writership.Mark();
#endif
            MarkSelfDirty();
            return cells[engine.WriteCellIndex];
        }

        public void CopyCells(int from, int to)
        {
            cells[to].Clear();
            cells[to].AddRange(cells[from]);
        }

        public void ClearCell(int at) { }

        private void MarkSelfDirty()
        {
            engine.MarkDirty(this);
        }
    }

    public class Op<T> : IHaveCells
    {
        private readonly Engine engine;
        private readonly List<T>[] cells;

        public Op(Engine engine)
        {
            this.engine = engine;

            cells = new List<T>[engine.TotalCells];
            for (int i = 0, n = cells.Length; i < n; ++i)
            {
                var l = new List<T>();
                cells[i] = l;
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

        public void CopyCells(int from, int to)
        {
            cells[to].Clear();
            cells[to].AddRange(cells[from]);
        }

        public void ClearCell(int at)
        {
            cells[at].Clear();
        }

        private void MarkSelfDirty()
        {
            engine.MarkDirty(this);
        }
    }

    public struct Void { }

    public static class TestCase01
    {
        public static void Run()
        {
            var e = new Engine();
            var name = e.El("jan");
            var age = e.El(1);
            var changeName = e.Op<Void>();

            e.RegisterComputer(new object[] { name }, () =>
            {
                age.Write(age.Read() + 1);
            });
            e.RegisterComputer(new object[] { changeName }, () =>
            {
                UnityEngine.Debug.Log("changeName count" + changeName.Read().Count);
                name.Write("new name");
            });
            e.RegisterListener(new object[] { age }, () =>
            {
                UnityEngine.Debug.Log("Age: " + age.Read());
            });

            changeName.Fire(default(Void));

            e.Update();
            e.Update();

            e.Dispose();
        }
    }
}
