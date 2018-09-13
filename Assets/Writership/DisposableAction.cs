using System;

namespace Writership
{
    public class DisposableAction : IDisposable
    {
        private Action action;

        public DisposableAction(Action action)
        {
            this.action = action;
        }

        public void Dispose()
        {
            if (action != null)
            {
                action();
                action = null;
            }
        }
    }
}
