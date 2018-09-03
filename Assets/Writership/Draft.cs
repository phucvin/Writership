using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Writership
{
    public class Engine
    {
        private class Dirty
        {
            public int Phase;
            public IHaveCells Inner;
        }

        private readonly List<Dirty>[] dirties;
        private readonly Dictionary<object, List<Action>>[] listeners;

        public Engine()
        {
            Cells = 2;
            dirties = new List<Dirty>[Cells];
            listeners = new Dictionary<object, List<Action>>[Cells];

            for (int i = 0, n = Cells; i < n; ++i)
            {
                dirties[i] = new List<Dirty>();
                listeners[i] = new Dictionary<object, List<Action>>();
            }
        }

        public int Cells { get; private set; }

        public int CurrentCellIndex
        {
            get
            {
                return 0;
            }
        }

        public int WriteCellIndex
        {
            get
            {
                return 1;
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
            var listeners = this.listeners[CurrentCellIndex];
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
        }

        public void Run(int at)
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
                    if (dirty.Phase != 2) continue;
                    dirty.Phase = 3;

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
                    if (dirty.Phase != 3) continue;
                    dirty.Phase = 4;

                    dirty.Inner.ClearCell(CurrentCellIndex);
                }
            }
            while (calledJobs.Count > 0);

            dirties.Clear();
            UnityEngine.Debug.Log("Engine ran: " + ran);
        }

        public ElP<T> ElP<T>(T value) where T : struct
        {
            return new ElP<T>(this, value);
        }

        public ElC<T> ElC<T>(T value) where T : ICloneable
        {
            return new ElC<T>(this, value);
        }

        public Op<T> Op<T>() where T : struct
        {
            return new Op<T>(this);
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
            var frame = new StackFrame(2, true);
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

    public class ElP<T> : IHaveCells
        where T : struct
    {
        private readonly Engine engine;
        private readonly T[] cells;

#if DEBUG
        private readonly Writership writership = new Writership();
#endif

        public ElP(Engine engine, T value)
        {
            this.engine = engine;

            cells = new T[engine.Cells];
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

    public class ElC<T> : IHaveCells
        where T : ICloneable
    {
        private readonly Engine engine;
        private readonly T[] cells;

#if DEBUG
        private readonly Writership writership = new Writership();
#endif

        public ElC(Engine engine, T value)
        {
            this.engine = engine;

            cells = new T[engine.Cells];
            for (int i = 0, n = cells.Length; i < n; ++i)
            {
                cells[i] = i == 0 ? value : (T)value.Clone();
            }
        }

        public T Read()
        {
            // TODO Return directly in release, to improve performance
            return (T)cells[engine.CurrentCellIndex].Clone();
        }

        public void Write(T value)
        {
#if DEBUG
            writership.Mark();
#endif
            MarkSelfDirty();
            cells[engine.WriteCellIndex] = value;
        }

        public T AsWrite()
        {
#if DEBUG
            writership.Mark();
#endif
            MarkSelfDirty();
            return cells[engine.WriteCellIndex];
        }

        public void CopyCells(int from, int to)
        {
            cells[to] = (T)cells[from].Clone();
        }

        public void ClearCell(int at) { }

        private void MarkSelfDirty()
        {
            engine.MarkDirty(this);
        }
    }

    public class LiP<T> : IHaveCells
        where T : struct
    {
        private readonly Engine engine;
        private readonly List<T>[] cells;

#if DEBUG
        private readonly Writership writership = new Writership();
#endif

        public LiP(Engine engine, IList<T> list)
        {
            this.engine = engine;

            cells = new List<T>[engine.Cells];
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
        where T : struct
    {
        private readonly Engine engine;
        private readonly List<T>[] cells;

        public Op(Engine engine)
        {
            this.engine = engine;

            cells = new List<T>[engine.Cells];
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
            var name = e.ElC("jan");
            var age = e.ElP(1);
            var changeName = e.Op<Void>();

            e.RegisterListener(new object[] { name }, () =>
            {
                age.Write(age.Read() + 1);
            });
            e.RegisterListener(new object[] { changeName }, () =>
            {
                name.Write("new name");
            });
            e.RegisterListener(new object[] { age }, () =>
            {
                UnityEngine.Debug.Log("Age: " + age.Read());
            });

            changeName.Fire(default(Void));

            e.Run(0);
        }
    }
}
