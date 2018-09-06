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
            Dispose();

            cd.Add(engine = new MultithreadEngine());
            cd.Add(state = new State(engine));

            cd.Add(Common.Binders.Label(engine, map.GetComponent<Text>("value"),
                state.Value, i => i.ToString()));
            cd.Add(Common.Binders.ButtonClick(engine, map.GetComponent<Button>("inc"),
                state.Increase, () => default(Empty)));
            cd.Add(Common.Binders.ButtonClick(engine, map.GetComponent<Button>("dec"),
                state.Decrease, () => default(Empty)));
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