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

        public static Op<T> Op<T>(this IEngine engine)
        {
            return new Op<T>(engine);
        }
    }
}
