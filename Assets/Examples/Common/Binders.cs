using System;
using UnityEngine.Events;
using UnityEngine.UI;
using Writership;

namespace Examples.Common
{
    public static class Binders
    {
        public static IDisposable Label<T>(IEngine engine, Text dst, IEl<T> src, Func<T, string> converter)
        {
            return engine.RegisterListener(new object[] { src }, () =>
                dst.text = converter(src.Read()));
        }

        public static IDisposable ButtonClick<T>(IEngine engine, Button src, IOp<T> dst, Func<T> valueGetter)
        {
            UnityAction action = () => dst.Fire(valueGetter());
            src.onClick.AddListener(action);
            return new RemoveOnClickListener(src, action);
        }
    }
}