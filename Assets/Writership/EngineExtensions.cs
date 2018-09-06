using System.Collections.Generic;

namespace Writership
{
    public static class EngineExtensions
    {
        public static El<T> El<T>(this MultithreadEngine engine, T value)
        {
            return new El<T>(engine, value);
        }

        public static Li<T> Li<T>(this MultithreadEngine engine, IList<T> list)
        {
            return new Li<T>(engine, list);
        }

        public static Op<T> Op<T>(this MultithreadEngine engine)
        {
            return new Op<T>(engine);
        }
    }
}
