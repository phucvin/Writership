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
        private Networker networker3;

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
            int view3 = 2;
            bool isDedicatedServer = true;
            if (!isDedicatedServer)
            {
                server = 1;
                view1 = 1;
                view2 = 2;
                view3 = -1;
            }

            networker1 = new Networker(engine, view1, server);
            var tankA1 = new Tank(engine, 1);
            tankA1.Setup(cd, engine, networker1);
            tankA1.SetupUnity(cd, engine, networker1, "plane1");
            var tankB1 = new Tank(engine, 2);
            tankB1.Setup(cd, engine, networker1);
            tankB1.SetupUnity(cd, engine, networker1, "plane1");

            networker2 = new Networker(engine, view2, server);
            var tankA2 = new Tank(engine, 1);
            tankA2.Setup(cd, engine, networker2);
            tankA2.SetupUnity(cd, engine, networker2, "plane2");
            var tankB2 = new Tank(engine, 2);
            tankB2.Setup(cd, engine, networker2);
            tankB2.SetupUnity(cd, engine, networker2, "plane2");

            networker3 = new Networker(engine, view3, server);
            if (view3 >= 0)
            {
                var tankA3 = new Tank(engine, 1);
                tankA3.Setup(cd, engine, networker3);
                tankA3.SetupUnity(cd, engine, networker3, "plane3");
                var tankB3 = new Tank(engine, 2);
                tankB3.Setup(cd, engine, networker3);
                tankB3.SetupUnity(cd, engine, networker3, "plane3");
            }
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
                networker1.TransferTo(networker2, networker3);
                networker2.TransferTo(networker1);
                networker3.TransferTo(networker1);
                simulateLag = 0f;
            }
            engine.Update();
        }
    }
}