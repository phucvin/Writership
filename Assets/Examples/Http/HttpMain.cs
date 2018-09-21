using System;
using UnityEngine;
using Writership;

namespace Examples.Http
{
    public class HttpMain : MonoBehaviour, IDisposable
    {
        public static HttpMain Instance { get; private set; }

        private IEngine engine;
        private State state;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public void Awake()
        {
            Instance = this;
        }

        public void Setup()
        {
            engine = new SinglethreadEngine();
            state = new State(engine);

            state.Setup(cd, engine);

            engine.Reader(cd, Dep.On(state.HttpUserId.Error, state.HttpUserId.Response), () =>
            {
                if (state.HttpUserId.Error) Debug.Log(state.HttpUserId.Error[0]);
                if (state.HttpUserId.Response) Debug.Log(state.HttpUserId.Response[0]);
            });

            state.HttpUserId.Request.Fire("phucvin");
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