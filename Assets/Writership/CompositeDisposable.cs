using System;
using System.Collections.Generic;

namespace Writership
{
    public class CompositeDisposable : IDisposable
    {
        private readonly List<IDisposable> list;

        public CompositeDisposable()
        {
            list = new List<IDisposable>();
        }

        public void Add(IDisposable item)
        {
            list.Add(item);
        }

        public T Add<T>(T item) where T : IDisposable
        {
            list.Add(item);
            return item;
        }

        public void Dispose()
        {
            for (int i = 0, n = list.Count; i < n; ++i)
            {
                list[i].Dispose();
            }
            list.Clear();
        }
    }
}
