using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Writership;

namespace Examples.Scenes
{
    public static class ListExtensions
    {
        public static void RemoveExact<T>(this List<T> list, T item)
        {
            if (!list.Remove(item)) throw new NotImplementedException();
        }

        public static void RemoveExact<T>(this List<T> list, IList<T> items)
        {
            int oldCount = list.Count;
            list.RemoveAll(it => items.Contains(it));
            if (list.Count - oldCount != items.Count) throw new NotImplementedException();
        }
    }

    public class CoroutineExecutor : MonoBehaviour
    {
        public static CoroutineExecutor Instance { get; private set; }

        public void Awake()
        {
            Instance = this;
        }
    }

    public class State
    {
        public readonly Data Data;
        public readonly Profile Profile;

        public readonly Home Home;
        public readonly Inventory Inventory;

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            engine.Worker(cd, Dep.On(
                Inventory.SellItem.Yes, Inventory.SellItems.Yes,
                Inventory.FuseItem.Yes), () =>
            {
                var items = Profile.Items.AsWriteProxy();
                if (Inventory.SellItem.Yes)
                {
                    items.RemoveExact(Inventory.ViewingItem);
                }
                if (Inventory.SellItems.Yes)
                {
                    items.RemoveExact(Inventory.SellingItems.Read());
                }
                if (Inventory.FuseItem.Yes)
                {
                    items.Add(Inventory.FuseItemResult);
                }
                items.Commit();
            });
        }

        public void SetupUnity(CompositeDisposable cd, IEngine engine)
        {
            engine.Mainer(cd, Dep.On(Home.Scene.Root), () =>
            {
                var root = Home.Scene.Root.Read();
                if (!root) return;
                var map = root.GetComponent<Common.Map>();
                var scd = root.GetComponent<Common.DisposeOnDestroy>().cd;

                Common.Binders.Label(scd, engine,
                    map.GetComponent<Text>("gold"), Profile.Gold,
                    i => string.Format("Gold: {0}", i)
                );
            });
        }
    }

    public class Data
    {
        public readonly IList<int> ItemUpgradeRequiredGoldByLevel;
    }

    public class Profile
    {
        public readonly El<int> Gold;
        public readonly Li<Item> Items;
    }

    public class Item
    {
        public readonly El<int> Id;
        public readonly string Name;
        public readonly El<int> Level;
    }

    public enum SceneState
    {
        Closed,
        Opening,
        Opened,
        Closing
    }

    public class Scene
    {
        public readonly string Name;
        public readonly LoadSceneMode Mode;
        public readonly El<SceneState> State;
        public readonly El<GameObject> Root;
        public readonly El<float> LoadingProgress;
        public readonly Op<Empty> Open;
        public readonly Op<Empty> Close;

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            engine.Worker(cd, Dep.On(Open, Close, Root), () =>
            {
                if (State == SceneState.Opening && Root.Read())
                {
                    State.Write(SceneState.Opened);
                }
                else if (State == SceneState.Closing && !Root.Read())
                {
                    State.Write(SceneState.Closed);
                }
                else if (State == SceneState.Closed && Open)
                {
                    State.Write(SceneState.Opening);
                }
                else if (State == SceneState.Opened && Close)
                {
                    State.Write(SceneState.Closing);
                }
                else throw new NotImplementedException();
            });
        }

        public void SetupUnity(CompositeDisposable cd, IEngine engine)
        {
            engine.Mainer(cd, Dep.On(State), () =>
            {
                if (State == SceneState.Opening)
                {
                    CoroutineExecutor.Instance.StartCoroutine(Exec(true));
                }
                else if (State == SceneState.Closing)
                {
                    CoroutineExecutor.Instance.StartCoroutine(Exec(false));
                }
            });
        }

        private IEnumerator Exec(bool open)
        {
            if (!open)
            {
                // TODO Trigger closing transition, wait for done
                UnityEngine.Object.Destroy(Root.Read());
                Root.Write(null);

                yield break;
            }

            var load = SceneManager.LoadSceneAsync(Name, Mode);
            while (!load.isDone)
            {
                LoadingProgress.Write(load.progress);
                yield return null;
            }

            var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            var root = scene.GetRootGameObjects()[0];
            // TODO Trigger opening transition, wait for done
            Root.Write(root);

            // Watch
            while (root) yield return null;
            Root.Write(null);
        }
    }

    public class Home
    {
        public readonly Scene Scene;
        public readonly Op<Empty> OpenInventory;
    }

    public class Inventory
    {
        public readonly Scene Scene;

        public readonly Li<Item> SortedItems;
        public readonly Op<int> ChangeSort;

        public readonly El<Item> ViewingItem;
        public readonly VerifyOp<Empty> UpgradeItem;
        public readonly ConfirmOp<Empty> SellItem;

        public readonly El<Item> FuseItemA;
        public readonly El<Item> FuseItemB;
        public readonly El<Item> FuseItemResult;
        public readonly VerifyConfirmOp<Empty> FuseItem;

        public readonly Li<Item> SellingItems;
        public readonly ConfirmOp<Empty> SellItems;
    }

    public class ConfirmOp<T>
    {
        public readonly Op<T> Trigger;
        public readonly Op<T> Yes;
        public readonly Op<T> No;
    }

    public class VerifyOp<T>
    {
        public readonly El<bool> Status;
        public readonly Op<T> Trigger;
        public readonly Op<T> Verified;
        public readonly Op<T> Rejected;
    }

    public class VerifyConfirmOp<T>
    {
        public readonly El<bool> Status;
        public readonly Op<T> Trigger;
        public readonly Op<T> Yes;
        public readonly Op<T> No;
        public readonly Op<T> Rejected;
    }
}