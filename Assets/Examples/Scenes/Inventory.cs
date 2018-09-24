using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Writership;

namespace Examples.Scenes
{
    public class Inventory
    {
        public readonly Scene<Empty> Scene;
        public readonly Li<Item> Items;
        public readonly Item.Factory ItemFactory;
        public readonly El<Item> RawSelectedItem;
        public readonly El<Item> SelectedItem;
        public readonly VerifyConfirmOp<Item> UpgradeItem;
        public readonly VerifyConfirmOp<Item> SellItem;

        public Inventory(IEngine engine)
        {
            Scene = new MyScene(engine, this);
            Items = engine.Li(new List<Item>());
            ItemFactory = new Item.Factory();
            RawSelectedItem = engine.El<Item>(null);
            SelectedItem = engine.El<Item>(null);
            UpgradeItem = new VerifyConfirmOp<Item>(engine,
                item => string.Format("Do you want to upgrade {0}?", item.Name)
            );
            SellItem = new VerifyConfirmOp<Item>(engine,
                item => string.Format("Do you want to sell {0}?", item.Name)
            );
        }

        public void Setup(CompositeDisposable cd, IEngine engine, State state)
        {
            Scene.Setup(cd, engine, state.SceneStack);
            ItemFactory.Setup(cd, engine, state);
            UpgradeItem.Setup(cd, engine, state.SceneStack);
            SellItem.Setup(cd, engine, state.SceneStack);

            engine.Worker(cd, Dep.On(state.Gold, SelectedItem), () =>
            {
                UpgradeItem.Status.Write(state.Gold >= 10 && SelectedItem.Read() != null);
            });
            engine.Worker(cd, Dep.On(SellItem.Current, Items), () =>
            {
                var current = SellItem.Current.Read();
                SellItem.Status.Write(current == null || Items.Read().Contains(current));
            });
            int nextId = 1;
            engine.Worker(cd, Dep.On(Scene.Open, SellItem.Yes), () =>
            {
                var items = Items.AsWriteProxy();
                if (Scene.Open)
                {
                    items.Add(ItemFactory.Create(
                        nextId.ToString(), "Sword " + nextId,
                        "item_" + ((nextId - 1) % 3 + 1), 1
                    ));
                    ++nextId;
                }
                Item item;
                if (SellItem.Yes.TryRead(out item))
                {
                    ItemFactory.Dispose(item);
                    // TODO Should be remove exact
                    items.Remove(item);
                }
                items.Commit();
            });
            engine.Worker(cd, Dep.On(RawSelectedItem, Items), () =>
            {
                var items = Items.Read();
                if (!items.Contains(SelectedItem))
                {
                    SelectedItem.Write(null);
                }
                else if (Items.Read().Contains(RawSelectedItem))
                {
                    SelectedItem.Write(RawSelectedItem);
                }
            });
            engine.Worker(cd, Dep.On(UpgradeItem.Dialog.Back), () =>
            {
                var back = UpgradeItem.Dialog.Back;
                if (!back || !back.Unwrap) return;
                UpgradeItem.Dialog.Close.Fire(Empty.Instance);
            });

#if DEBUG
            engine.OpWorker(cd, Dep.On(UpgradeItem.Trigger), () =>
            {
                if (!Items.Read().Contains(UpgradeItem.Trigger.Unwrap))
                {
                    throw new NotImplementedException();
                }
            });
            engine.OpWorker(cd, Dep.On(SellItem.Trigger), () =>
            {
                if (!Items.Read().Contains(SellItem.Trigger.Unwrap))
                {
                    throw new NotImplementedException();
                }
            });
#endif
        }

        public void SetupUnity(CompositeDisposable cd, IEngine engine, State state)
        {
            Scene.SetupUnity(cd, engine);
            UpgradeItem.SetupUnity(cd, engine);
            SellItem.SetupUnity(cd, engine);

            engine.Mainer(cd, Dep.On(Scene.Root), () =>
            {
                var root = Scene.Root.Read();
                if (!root) return;
                var map = root.GetComponent<Common.Map>();
                var scd = root.GetComponent<Common.DisposeOnDestroy>().cd;

                Common.Binders.Label(scd, engine,
                    map.GetComponent<Text>("gold"), state.Gold,
                    i => string.Format("Gold: {0}", i)
                );
                Common.Binders.Click(scd, engine,
                    map.GetComponent<Common.Clickable>("upgrade"), UpgradeItem.Trigger,
                    () => SelectedItem.Read()
                );
                Common.Binders.ButtonInteractable(scd, engine,
                    map.GetComponent<Button>("upgrade"), UpgradeItem.Status,
                    b => b
                );
                Common.Binders.List(scd, engine,
                    map.GetComponent<Transform>("items"),
                    map.GetComponent<Common.Map>("item"),
                    Items, (icd, imap, item) =>
                    {
                        Common.Binders.Label(icd, engine,
                            imap.GetComponent<Text>("level"), item.Level,
                            i => string.Format("Lv.{0}", i)
                        );
                        Common.Binders.Image(icd, engine,
                            imap.GetComponent<Image>("image"), item.Image
                        );
                        Common.Binders.ButtonClick(icd, engine,
                            imap.GetComponent<Button>("view_details"),
                            () => ItemDetails.CreateAndShow(engine, state, item)
                        );
                        Common.Binders.Click(icd, engine,
                            imap.GetComponent<Common.Clickable>("select"),
                            () => RawSelectedItem.Write(
                                RawSelectedItem.Read() == item ? null : item)
                        );
                        Common.Binders.Enabled(icd, engine,
                            imap.Get("is_selected"), SelectedItem,
                            it => it == item
                        );
                    }
                );
            });
        }

        private class MyScene : Scene<Empty>
        {
            private readonly Inventory inventory;

            public MyScene(IEngine engine, Inventory inventory)
                : base(engine, "Inventory")
            {
                this.inventory = inventory;
            }

            protected override IEnumerator<float> Preload()
            {
                var items = inventory.Items.Read();
                var requests = new List<AsyncOperation>();
                for (int i = 0, n = items.Count; i < n; ++i)
                {
                    requests.Add(Resources.LoadAsync(items[i].Image));
                }
                return Preload(requests);
            }

            private static IEnumerator<float> Preload(IList<AsyncOperation> requests)
            {
                while (true)
                {
                    bool done = true;
                    float progress = 0f;
                    for (int i = 0, n = requests.Count; i < n; ++i)
                    {
                        var r = requests[i];
                        done &= r.isDone;
                        progress += r.progress;
                    }
                    if (done) break;
                    else yield return progress / requests.Count;
                }
                yield return 1f;
            }
        }
    }
}