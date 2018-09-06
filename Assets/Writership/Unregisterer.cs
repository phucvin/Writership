using System;

namespace Writership
{
    public class Unregisterer : IDisposable
    {
        private readonly IEngine engine;
        private readonly int at;
        private readonly object[] targets;
        private readonly Action job;

        public Unregisterer(IEngine engine, int at, object[] targets, Action job)
        {
            this.engine = engine;
            this.at = at;
            this.targets = targets;
            this.job = job;
        }

        public void Dispose()
        {
            engine.UnregisterListener(at, targets, job);
        }
    }
}
