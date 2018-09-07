using System;
using Writership;

namespace Examples.SimpleBattle
{
    public class Disposable : IDisposable
    {
        protected readonly CompositeDisposable cd = new CompositeDisposable();

        public void Dispose()
        {
            cd.Dispose();
        }
    }
}