using UnityEngine;
using Writership;

namespace Examples.Counter
{
    public class Main : MonoBehaviour
    {
        [SerializeField]
        private BasicLabel valueLabel = null;

        [SerializeField]
        private BasicButton increaseButton = null;

        [SerializeField]
        private BasicButton decreaseButton = null;

        private Engine engine;
        private State state;

        public void Start()
        {
            engine = new Engine();
            state = new State(engine);

            valueLabel.Setup(engine, state.Value);
            increaseButton.Setup(engine, state.Increase);
            decreaseButton.Setup(engine, state.Decrease);
        }

        public void Update()
        {
            engine.Update();
        }

        public void OnDestroy()
        {
            engine.Dispose();
        }
    }
}