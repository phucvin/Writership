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
        public static void Label<T>(CompositeDisposable cd, IEngine engine,
            Text dst, IEl<T> src, Func<T, string> converter)
        {
            if (!dst) return;

            cd.Add(engine.RegisterListener(
                new object[] { src },
                () => dst.text = converter(src.Read())
            ));
        }

        public static void ButtonClick<T>(CompositeDisposable cd, IEngine engine,
            Button src, IOp<T> dst, Func<T> valueGetter)
        {
            if (!src) return;

            UnityAction action = () => dst.Fire(valueGetter());
            src.onClick.AddListener(action);
            cd.Add(new RemoveOnClickListener(src, action));
        }

        public static void List<T>(CompositeDisposable cd, IEngine engine,
            Transform dst, Map prefab, ILi<T> src,
            Action<CompositeDisposable, Map, T> itemBinder)
        {
            if (!dst || !prefab) return;

            var createdGameObjects = new List<GameObject>();
            var createdCd = new CompositeDisposable();

            cd.Add(createdCd);
            cd.Add(engine.RegisterListener(
                new object[] { src },
                () =>
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
                        var itemCd = new CompositeDisposable();
                        createdCd.Add(itemCd);

                        itemBinder(itemCd, itemMap, items[i]);
                        createdGameObjects.Add(itemMap.gameObject);
                    }
                }
            ));
        }
    }
}