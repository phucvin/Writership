using UnityEngine;
using UnityEngine.UI;
using Writership;

namespace Examples.Counter
{
    public class BasicButton : MonoBehaviour
    {
        [SerializeField]
        private Button button = null;

        // TODO Disposable
        public void Setup(Engine engine, Op<Void> op)
        {
            button.onClick.AddListener(() => op.Fire(default(Void)));
        }
    }
}