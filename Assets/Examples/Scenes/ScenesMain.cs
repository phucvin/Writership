using System;
using UnityEngine;
using Writership;

namespace Examples.Scenes
{
    public class ScenesMain : MonoBehaviour, IDisposable
    {
        [SerializeField]
        private Common.Map map = null;

        private IEngine engine;
        private State state;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public void Setup()
        {
            engine = new SinglethreadEngine();
            state = new State();

            state.Setup(cd, engine);
            state.SetupUnity(cd, engine);
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