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

        public static Watcher Watcher(this IEngine engine)
        {
            return new Watcher(engine);
        }
    }
}
