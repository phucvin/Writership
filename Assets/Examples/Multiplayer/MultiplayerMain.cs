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

            var networker = new Networker(engine, 0, 0);
            var tank = new Tank(engine, 0);

            tank.Setup(cd, engine, networker);
            tank.SetupTest(cd, engine, networker);
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