using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Writership;

namespace Examples.Common
{
    public class BasicButton : MonoBehaviour, IDisposable
    {
        [SerializeField]
        private Button button = null;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public IDisposable Setup(IEngine engine, IOp<Empty> op)
        {
            Dispose();

            UnityAction action = () => op.Fire(default(Empty));
            button.onClick.AddListener(action);
            cd.Add(new RemoveOnClickListener(button, action));

            return this;
        }

        public void Dispose()
        {
            cd.Dispose();
        }

        public void OnDestroy()
        {
            Dispose();
        }

        private class RemoveOnClickListener : IDisposable
        {
            private readonly Button button;
            private readonly UnityAction action;

            public RemoveOnClickListener(Button button, UnityAction action)
            {
                this.button = button;
                this.action = action;
            }

            public void Dispose()
            {
                button.onClick.RemoveListener(action);
            }
        }
    }
}