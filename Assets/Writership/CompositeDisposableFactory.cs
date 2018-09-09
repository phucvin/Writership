using System.Collections.Generic;

namespace Writership
{
    public abstract class CompositeDisposableFactory<T>
    {
        private readonly Dictionary<T, CompositeDisposable> map =
            new Dictionary<T, CompositeDisposable>();

        protected CompositeDisposable Add(T item)
        {
            var cd = new CompositeDisposable();
            map.Add(item, cd);
            return cd;
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