using System;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Examples.Common
{
    public class RemoveOnValueChangedListener : IDisposable
    {
        private readonly InputField input;
        private readonly UnityAction<string> action;

        public RemoveOnValueChangedListener(InputField input, UnityAction<string> action)
        {
            this.input = input;
            this.action = action;
        }

        public void Dispose()
        {
            input.onValueChanged.RemoveListener(action);
        }
    }
}