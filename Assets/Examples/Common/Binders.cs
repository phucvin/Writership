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
        public static bool Enabled(CompositeDisposable cd, IEngine engine,
            GameObject dst, IReadable<bool> src)
        {
            if (!dst) return NotBinded();

            engine.Reader(cd,
                new object[] { src },
                () => dst.SetActive(src.Read())
            );
            return true;
        }

        public static bool Enabled<T>(CompositeDisposable cd, IEngine engine,
            GameObject dst, IReadable<T> src,
            Func<T, bool> converter)
        {
            if (!dst) return NotBinded();

            engine.Reader(cd,
                new object[] { src },
                () => dst.SetActive(converter(src.Read()))
            );
            return true;
        }

        public static bool Label<T>(CompositeDisposable cd, IEngine engine,
            Text dst, IReadable<T> src,
            Func<T, string> converter)
        {
            if (!dst) return NotBinded();

            engine.Reader(cd,
                new object[] { src },
                () => dst.text = converter(src.Read())
            );
            return true;
        }

        public static bool Label(CompositeDisposable cd, IEngine engine,
            Text dst, string text)
        {
            if (!dst) return NotBinded();

            dst.text = text;
            return true;
        }

        public static bool InputField<T>(CompositeDisposable cd, IEngine engine,
            InputField src, IWriteable<T> dst,
            Func<string, T> converter)
        {
            if (!src) return NotBinded();
            
            UnityAction<string> action = text => dst.Write(converter(text));
            src.onValueChanged.AddListener(action);
            cd.Add(new DisposableAction(() => src.onValueChanged.RemoveListener(action)));
            return true;
        }

        public static bool InputFieldTwoWay<T>(CompositeDisposable cd, IEngine engine,
            InputField mid,
            IWriteable<T> dst, Func<string, T> dstConverter,
            IReadable<T> src, Func<T, string> srcConverter)
        {
            if (!mid) return NotBinded();

            engine.Reader(cd, new object[] { src }, () =>
            {
                if (Equals(dstConverter(mid.text), src.Read())) return;
                mid.text = srcConverter(src.Read());
            });

            UnityAction<string> action = text =>
            {
                if (srcConverter(src.Read()) == text) return;
                dst.Write(dstConverter(text));
            };
            mid.onValueChanged.AddListener(action);
            cd.Add(new DisposableAction(() => mid.onValueChanged.RemoveListener(action)));

            return true;
        }

        public static bool ButtonClick<T>(CompositeDisposable cd, IEngine engine,
            Button src, IFireable<T> dst,
            Func<T> valueGetter, Func<bool> checker = null)
        {
            if (!src) return NotBinded();

            UnityAction action = () =>
            {
                if (checker != null && !checker()) return;
                dst.Fire(valueGetter());
            };
            src.onClick.AddListener(action);
            cd.Add(new DisposableAction(() => src.onClick.RemoveListener(action)));
            return true;
        }

        public static bool ButtonClick(CompositeDisposable cd, IEngine engine,
            Button src, UnityAction action)
        {
            if (!src) return NotBinded();

            src.onClick.AddListener(action);
            cd.Add(new DisposableAction(() => src.onClick.RemoveListener(action)));
            return true;
        }

        public static bool Click<T>(CompositeDisposable cd, IEngine engine,
            Clickable src, IFireable<T> dst,
            Func<T> valueGetter, Func<bool> checker = null)
        {
            if (!src) return NotBinded();

            UnityAction action = () =>
            {
                if (checker != null && !checker()) return;
                dst.Fire(valueGetter());
            };
            src.onClick.AddListener(action);
            cd.Add(new DisposableAction(() => src.onClick.RemoveListener(action)));
            return true;
        }

        public static bool Click(CompositeDisposable cd, IEngine engine,
            Clickable src, UnityAction action)
        {
            if (!src) return NotBinded();
            
            src.onClick.AddListener(action);
            cd.Add(new DisposableAction(() => src.onClick.RemoveListener(action)));
            return true;
        }

        public static bool ToggleIsOn<T>(CompositeDisposable cd, IEngine engine,
            Toggle dst, IReadable<T> src,
            Func<T, bool> converter)
        {
            if (!dst) return NotBinded();

            engine.Reader(cd,
                new object[] { src },
                () => dst.isOn = converter(src.Read())
            );
            return true;
        }

        public static bool ToggleChange(CompositeDisposable cd, IEngine engine,
            Toggle src, UnityAction<bool> action)
        {
            if (!src) return NotBinded();

            src.onValueChanged.AddListener(action);
            cd.Add(new DisposableAction(() => src.onValueChanged.RemoveListener(action)));
            return true;
        }

        public static bool TextColor<T>(CompositeDisposable cd, IEngine engine,
            Text dst, IReadable<T> src,
            Func<T, Color> converter)
        {
            if (!dst) return NotBinded();

            engine.Reader(cd,
                new object[] { src },
                () => dst.color = converter(src.Read())
            );
            return true;
        }

        public static bool Interactable<T>(CompositeDisposable cd, IEngine engine,
            Selectable dst, IReadable<T> src,
            Func<T, bool> converter)
        {
            if (!dst) return NotBinded();

            engine.Reader(cd,
                new object[] { src },
                () => dst.interactable = converter(src.Read())
            );
            return true;
        }

        public static bool Image(CompositeDisposable cd, IEngine engine,
            Image dst, string resourcePath)
        {
            if (!dst) return NotBinded();

            dst.sprite = Resources.Load<Sprite>(resourcePath);
            return true;
        }

        public static bool Tabs<T>(CompositeDisposable cd, IEngine engine,
            Map map, string[] tabs, string[] toggles,
            IWriteable<T> dst, Func<string, T> dstConverter,
            IReadable<T> src, Func<T, string> srcConverter)
        {
            engine.Reader(cd, Dep.On(src), () =>
            {
                string selected = srcConverter(src.Read());
                for (int i = 0, n = tabs.Length; i < n; ++i)
                {
                    // TODO Fade animation
                    map.Get(tabs[i]).SetActive(tabs[i] == selected);
                }
            });
            for (int i = 0, n = toggles.Length; i < n; ++i)
            {
                string tab = tabs[i];
                string toggle = toggles[i];
                ToggleIsOn(cd, engine,
                    map.GetComponent<Toggle>(toggles[i]), src,
                    t => srcConverter(t) == tab
                );
                ToggleChange(cd, engine,
                    map.GetComponent<Toggle>(toggles[i]),
                    b => { if (b) dst.Write(dstConverter(tab)); }
                );
            }
            return true;
        }

        public static bool List<T>(CompositeDisposable cd, IEngine engine,
            Transform dst, Map prefab, IReadable<IList<T>> src,
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