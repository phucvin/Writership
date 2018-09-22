using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Writership;

namespace Examples.TwoWay
{
    public class TwoWayMain : MonoBehaviour, IDisposable
    {
        [SerializeField]
        private Common.Map map = null;

        private IEngine engine;
        private El<string> way1;
        private El<string> way2;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public void Setup()
        {
            engine = new SinglethreadEngine();
            way1 = engine.El(string.Empty);
            way2 = engine.El(string.Empty);

            var sb = new StringBuilder();
            engine.Worker(cd, Dep.On(way1), () =>
            {
                sb.Length = 0;
                sb.Append(way1.Read());
                sb.Replace("hello", "HELLO");
                sb.Replace("bye", "");
                way2.Write(sb.ToString());
            });

            Common.Binders.InputField(cd, engine,
                map.GetComponent<InputField>("input"), way1,
                i => i.ToString()
            );
            Common.Binders.InputField2(cd, engine,
                map.GetComponent<InputField>("input"), way2,
                s => s
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