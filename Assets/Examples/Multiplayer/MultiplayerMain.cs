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

            int server = 0;
            int view1 = 0;
            int view2 = 1;
            bool isDedicatedServer = false;
            if (!isDedicatedServer)
            {
                server = 1;
                view1 = 1;
                view2 = 2;
            }

            networker1 = new Networker(engine, view1, server);
            var tank1 = new Tank(engine, 1);
            tank1.Setup(cd, engine, networker1);
            tank1.SetupUnity(cd, engine, networker1, "plane1");

            networker2 = new Networker(engine, view2, server);
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

        float simulateLag = 0;
        public void Update()
        {
            simulateLag += Time.deltaTime;
            if (simulateLag >= 0.1f)
            {
                networker1.TransferTo(networker2);
                networker2.TransferTo(networker1);
                simulateLag = 0f;
            }
            engine.Update();
        }
    }
}