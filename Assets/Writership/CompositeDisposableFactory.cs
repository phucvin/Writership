using System;
using System.Collections.Generic;

namespace Writership
{
    public abstract class CompositeDisposableFactory<T> : IDisposable
    {
        private readonly Dictionary<T, CompositeDisposable> map =
            new Dictionary<T, CompositeDisposable>();

        public void Dispose()
        {
            if (map.Count > 0)
            {
                foreach (var cd in map.Values)
                {
                    cd.Dispose();
                }
                map.Clear();
            }
        }

        protected CompositeDisposable Add(T item)
        {
            var cd = new CompositeDisposable();
            map.Add(item, cd);
            return cd;
        }

        protected void Add(T item, CompositeDisposable cd)
        {
            map.Add(item, cd);
        }

        protected CompositeDisposable Remove(T item)
        {
            CompositeDisposable cd = null;
            if (map.TryGetValue(item, out cd))
            {
                map.Remove(item);
                cd.Dispose();
            }
            return cd;
        }
    }
}