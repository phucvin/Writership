using System;
using System.Collections.Generic;

namespace Writership
{
    public static class EngineExtensions
    {
        public static IEl<T> El<T>(this IEngine engine, T value)
        {
            return new El<T>(engine, value);
        }

        public static ILi<T> Li<T>(this IEngine engine, IList<T> list)
        {
            return new Li<T>(engine, list);
        }

        public static IOp<T> Op<T>(this IEngine engine, bool allowWriters = false, bool needApplied = false)
        {
            return new Op<T>(engine, allowWriters, needApplied);
        }

        public static ILiWa LiWa<T>(this IEngine engine, CompositeDisposable cd, ILi<T> li, Func<T, object> extractor)
        {
            var liwa = new LiWa(engine);
            liwa.Setup(cd, engine, li, extractor);
            return liwa;
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

        public static void Guarder(this IEngine engine, CompositeDisposable cd, object[] targets, Action job)
        {
            engine.Listen(engine.MainCellIndex, cd, targets, job);
        }
    }
}
