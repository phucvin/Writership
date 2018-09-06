using UnityEngine;
using UnityEngine.UI;
using Writership;

namespace Examples.Counter
{
    public class BasicLabel : MonoBehaviour
    {
        [SerializeField]
        private Text text = null;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public void Setup(MultithreadEngine engine, El<int> target)
        {
            cd.Add(engine.RegisterListener(
                new object[] { target },
                () => text.text = target.Read().ToString()
            ));
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