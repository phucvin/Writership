using System;
using UnityEngine;
using Writership;

namespace Examples.Counter
{
    public class Main : MonoBehaviour, IDisposable
    {
        [SerializeField]
        private Common.BasicLabel valueLabel = null;

        [SerializeField]
        private Common.BasicButton increaseButton = null;

        [SerializeField]
        private Common.BasicButton decreaseButton = null;

        private IEngine engine;
        private State state;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public void Setup()
        {
            Dispose();

            cd.Add(engine = new MultithreadEngine());
            cd.Add(state = new State(engine));

            cd.Add(valueLabel.Setup(engine, state.Value));
            cd.Add(increaseButton.Setup(engine, state.Increase));
            cd.Add(decreaseButton.Setup(engine, state.Decrease));
        }

        public void Dispose()
        {
            cd.Dispose();
        }

        public void Start()
        {
            Setup();
        }

        public void OnDestroy()
        {
            Dispose();
        }

        public void Update()
        {
            engine.Update();
        }
    }
}