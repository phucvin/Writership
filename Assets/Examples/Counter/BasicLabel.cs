using UnityEngine;
using UnityEngine.UI;
using Writership;

namespace Examples.Counter
{
    public class BasicLabel : MonoBehaviour
    {
        [SerializeField]
        private Text text = null;

        // TODO Disposable
        public void Setup(Engine engine, El<int> target)
        {
            engine.RegisterListener(
                new object[] { target },
                () => text.text = target.Read().ToString()
            );
        }
    }
}