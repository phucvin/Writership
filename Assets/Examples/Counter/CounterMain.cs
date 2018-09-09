using System;
using UnityEngine;
using UnityEngine.UI;
using Writership;

namespace Examples.Counter
{
    public class CounterMain : MonoBehaviour, IDisposable
    {
        [SerializeField]
        private Common.Map map = null;

        private IEngine engine;
        private State state;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public void Setup()
        {
            engine = new MultithreadEngine();
            state = new State(cd, engine);

            Common.Binders.Label(cd, engine,
                map.GetComponent<Text>("value"), state.Value,
                i => i.ToString()
            );
            Common.Binders.ButtonClick(cd, engine,
                map.GetComponent<Button>("inc"), state.Increase,
                () => Empty.Instance
            );
            Common.Binders.ButtonClick(cd, engine,
                map.GetComponent<Button>("dec"), state.Decrease,
                () => Empty.Instance
            );
        }

        public void Dispose()
        {
            cd.Dispose();
            engine.Dispose();
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