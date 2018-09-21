using System;
using System.Collections.Generic;

namespace Writership
{
    public static class EngineExtensions
    {
        public static El<T> El<T>(this IEngine engine, T value)
        {
            return new El<T>(engine, value);
        }

        public static Li<T> Li<T>(this IEngine engine, IList<T> list)
        {
            return new Li<T>(engine, list);
        }

        public static Ar<T> Ar<T>(this IEngine engine, T[] array)
            where T : struct
        {
            return new Ar<T>(engine, array);
        }

        public static Op<T> Op<T>(this IEngine engine, bool allowWriters = false, bool needApplied = false, Func<T, T, T> reducer = null)
        {
            return new Op<T>(engine, allowWriters, needApplied, reducer);
        }

        public static Wa Wa<T>(this IEngine engine, CompositeDisposable cd, ILi<T> li, Func<T, object> extractor)
        {
            var liwa = new Wa(engine);
            liwa.Setup(cd, engine, li, extractor);
            return liwa;
        }

        public static void Mainer(this IEngine engine, CompositeDisposable cd, object[] targets, Action job)
        {
            engine.Listen(engine.MainCellIndex, cd, targets, job);
        }

        public static void Worker(this IEngine engine, CompositeDisposable cd, object[] targets, Action job)
        {
            engine.Listen(engine.WorkerCellIndex, cd, targets, job);
        }

        public static void Computer(this IEngine engine, CompositeDisposable cd, object[] targets, Action job)
        {
            engine.Listen(engine.WorkerCellIndex, cd, targets, job);
        }

        public static void Reader(this IEngine engine, CompositeDisposable cd, object[] targets, Action job)
        {
            engine.Listen(engine.MainCellIndex, cd, targets, job);
        }

        public static void Writer(this IEngine engine, CompositeDisposable cd, object[] targets, Action job)
        {
            engine.Listen(engine.MainCellIndex, cd, targets, job);
        }

        public static void Firer(this IEngine engine, CompositeDisposable cd, object[] targets, Action job)
        {
            engine.Listen(engine.MainCellIndex, cd, targets, job);
        }

        public static void Guarder(this IEngine engine, CompositeDisposable cd, object[] targets, Action job)
        {
            engine.Listen(engine.MainCellIndex, cd, targets, job);
        }
    }
}
