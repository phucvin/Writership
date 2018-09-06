using UnityEngine;
using UnityEngine.UI;
using Writership;

namespace Examples.Counter
{
    public class BasicButton : MonoBehaviour
    {
        [SerializeField]
        private Button button = null;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public void Setup(IEngine engine, Op<Empty> op)
        {
            button.onClick.AddListener(() => op.Fire(default(Empty)));
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