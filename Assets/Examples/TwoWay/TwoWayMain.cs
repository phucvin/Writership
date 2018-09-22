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
        private Tw<string> input;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public void Setup()
        {
            engine = new SinglethreadEngine();
            input = engine.Tw(string.Empty);

            var sb = new StringBuilder();
            engine.Worker(cd, Dep.On(input), () =>
            {
                sb.Length = 0;
                sb.Append(input.Read());
                sb.Replace("hello", "HELLO");
                sb.Replace("bye", "");
                input.Write(sb.ToString());
            });

            Common.Binders.InputFieldTwoWay(cd, engine,
                map.GetComponent<InputField>("input"), input,
                s => s, s => s
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