using UnityEngine;
using Writership;

namespace Examples.Common
{
    public class DisposeOnDestroy : MonoBehaviour
    {
        public readonly CompositeDisposable cd = new CompositeDisposable();

        public void OnDestroy()
        {
            cd.Dispose();
        }
    }
}