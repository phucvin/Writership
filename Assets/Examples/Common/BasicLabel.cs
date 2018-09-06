using System;
using UnityEngine;
using UnityEngine.UI;
using Writership;

namespace Examples.Common
{
    public class BasicLabel : MonoBehaviour, IDisposable
    {
        [SerializeField]
        private Text text = null;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public IDisposable Setup(IEngine engine, El<int> target)
        {
            Dispose();

            cd.Add(engine.RegisterListener(
                new object[] { target },
                () => text.text = target.Read().ToString()
            ));

            return this;
        }

        public void Dispose()
        {
            cd.Dispose();
        }

        public void OnDestroy()
        {
            Dispose();
        }
    }
}