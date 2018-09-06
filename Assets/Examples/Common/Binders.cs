using System;
using System.Collections.Generic;
using UnityEngine;
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

        public static IDisposable List<T>(IEngine engine, Transform dst, Map prefab, ILi<T> src, Func<Map, T, IDisposable> itemBinder)
        {
            var createdGameObjects = new List<GameObject>();
            var createdCd = new CompositeDisposable();
            return engine.RegisterListener(new object[] { src }, () =>
            {
                createdCd.Dispose();
                for (int i = 0, n = createdGameObjects.Count; i < n; ++i)
                {
                    UnityEngine.Object.Destroy(createdGameObjects[i]);
                }

                var items = src.Read();
                for (int i = 0, n = items.Count; i < n; ++i)
                {
                    var itemMap = UnityEngine.Object.Instantiate(prefab.gameObject, dst).GetComponent<Map>();
                    itemMap.gameObject.SetActive(true);

                    createdCd.Add(itemBinder(itemMap, items[i]));
                    createdGameObjects.Add(itemMap.gameObject);
                }
            });
        }
    }
}