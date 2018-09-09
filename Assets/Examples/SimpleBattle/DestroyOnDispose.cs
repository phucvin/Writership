using System;

namespace Examples.SimpleBattle
{
    public class DestroyOnDispose : IDisposable
    {
        private readonly UnityEngine.Object target;

        public DestroyOnDispose(UnityEngine.Object target)
        {
            this.target = target;
        }

        public void Dispose()
        {
            if (target) UnityEngine.Object.Destroy(target);
        }
    }
}