using System;
using UnityEngine;
using Writership;

namespace Examples.Multiplayer
{
    public class MultiplayerMain : MonoBehaviour, IDisposable
    {
        private IEngine engine;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public void Setup()
        {
            engine = new SinglethreadEngine();
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