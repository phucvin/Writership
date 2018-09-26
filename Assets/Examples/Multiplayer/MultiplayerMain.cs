using System;
using UnityEngine;
using Writership;

namespace Examples.Multiplayer
{
    public class MultiplayerMain : MonoBehaviour, IDisposable
    {
        public static MultiplayerMain Instance { get; private set; }

        [SerializeField]
        private Common.Map map = null;

        private IEngine engine;
        private Networker networker1;
        private Networker networker2;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public void Awake()
        {
            Instance = this;
        }

        public GameObject Get(string name)
        {
            return map.Get(name);
        }

        public void Setup()
        {
            engine = new SinglethreadEngine();

            networker1 = new Networker(engine, 0, 0);
            var tank1 = new Tank(engine, 1);
            tank1.Setup(cd, engine, networker1);
            tank1.SetupUnity(cd, engine, networker1, "plane1");

            networker2 = new Networker(engine, 1, 0);
            var tank2 = new Tank(engine, 1);
            tank2.Setup(cd, engine, networker2);
            tank2.SetupUnity(cd, engine, networker2, "plane2");
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
            networker1.TransferTo(networker2);
            networker2.TransferTo(networker1);
            engine.Update();
        }
    }
}