using System;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Examples.Common
{
    public class RemoveOnClickListener : IDisposable
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