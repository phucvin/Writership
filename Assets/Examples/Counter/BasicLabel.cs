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

        public void Setup(IEngine engine, El<int> target)
        {
            cd.Add(engine.RegisterListener(
                new object[] { target },
                () =>
                {
                    Debug.Log("Set text at frame: " + Time.frameCount);
                    text.text = target.Read().ToString();
                }
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