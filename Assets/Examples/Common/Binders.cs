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
        public static bool Label<T>(CompositeDisposable cd, IEngine engine,
            Text dst, IEl<T> src,
            Func<T, string> converter)
        {
            if (!dst) return NotBinded();

            engine.Reader(cd,
                new object[] { src },
                () => dst.text = converter(src.Read())
            );
            return true;
        }

        public static bool InputField<T>(CompositeDisposable cd, IEngine engine,
            InputField src, IEl<T> dst,
            Func<string, T> converter)
        {
            if (!src) return NotBinded();

            UnityAction<string> action = text => dst.Write(converter(text));
            src.onValueChanged.AddListener(action);
            cd.Add(new RemoveOnValueChangedListener(src, action));
            return true;
        }

        public static bool ButtonClick<T>(CompositeDisposable cd, IEngine engine,
            Button src, IOp<T> dst,
            Func<T> valueGetter)
        {
            if (!src) return NotBinded();

            UnityAction action = () => dst.Fire(valueGetter());
            src.onClick.AddListener(action);
            cd.Add(new RemoveOnClickListener(src, action));
            return true;
        }

        public static bool List<T>(CompositeDisposable cd, IEngine engine,
            Transform dst, Map prefab, ILi<T> src,
            Action<CompositeDisposable, Map, T> itemBinder)
        {
            if (!dst || !prefab) return NotBinded();

            var createdGameObjects = new List<GameObject>();
            var createdCd = new CompositeDisposable();

            cd.Add(createdCd);
            engine.Reader(cd,
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
            );
            return true;
        }

        private static bool NotBinded()
        {
            Debug.LogWarning("Something is not binded, check stack trace for details");
            return false;
        }
    }
}