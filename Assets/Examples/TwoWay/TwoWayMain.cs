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
        private El<string> rawInput;
        private El<string> input;
        private Op<Empty> randomize;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public void Setup()
        {
            engine = new SinglethreadEngine();
            rawInput = engine.El(string.Empty);
            input = engine.El(string.Empty);
            randomize = engine.Op<Empty>();

            var sb = new StringBuilder();
            var rand = new System.Random();
            engine.Worker(cd, Dep.On(rawInput, randomize), () =>
            {
                sb.Length = 0;
                sb.Append(rawInput.Read());
                sb.Replace("hello", "HELLO");
                sb.Replace("bye", "");

                if (randomize)
                {
                    for (int i = 0, n = sb.Length; i < n; ++i)
                    {
                        sb[i] += (char)rand.Next(-10, 10);
                    }
                }

                input.Write(sb.ToString());
            });

            Common.Binders.InputFieldTwoWay(cd, engine,
                map.GetComponent<InputField>("input"),
                rawInput, s => s,
                input, s => s
            );
            Common.Binders.ButtonClick(cd, engine,
                map.GetComponent<Button>("randomize"), randomize,
                () => Empty.Instance
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