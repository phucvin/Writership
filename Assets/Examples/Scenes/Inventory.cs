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
        public readonly El<Item> SelectedItem;
        public readonly VerifyConfirmOp<Item> UpgradeItem;
        public readonly VerifyConfirmOp<Item> SellItem;

        public Inventory(IEngine engine)
        {
            Scene = new MyScene(engine);
            Items = engine.Li(new List<Item>());
            ItemFactory = new Item.Factory();
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
            Scene.Setup(cd, engine);
            ItemFactory.Setup(cd, engine, state);
            UpgradeItem.Setup(cd, engine);
            SellItem.Setup(cd, engine);

            engine.Worker(cd, Dep.On(state.Gold, SelectedItem), () =>
            {
                UpgradeItem.Status.Write(state.Gold >= 10 && SelectedItem.Read() != null);
            });
            int nextId = 1;
            engine.Worker(cd, Dep.On(Scene.Open, SellItem.Yes), () =>
            {
                var items = Items.AsWriteProxy();
                if (Scene.Open)
                {
                    items.Add(ItemFactory.Create(
                        nextId.ToString(), "Sword " + nextId, "item_sword", 1
                    ));
                    ++nextId;
                }
                if (SellItem.Yes)
                {
                    ItemFactory.Dispose(SellItem.Yes.First);
                    // TODO Should be remove exact
                    items.Remove(SellItem.Yes.First);
                }
                items.Commit();
            });
            engine.Worker(cd, Dep.On(UpgradeItem.Dialog.Back), () =>
            {
                var back = UpgradeItem.Dialog.Back;
                if (!back || !back.First) return;
                UpgradeItem.Dialog.Close.Fire(Empty.Instance);
            });

#if DEBUG
            engine.OpWorker(cd, Dep.On(UpgradeItem.Trigger), () =>
            {
                if (!Items.Read().Contains(UpgradeItem.Trigger.First))
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
                    map.GetComponent<Common.Clickable>("back"), Scene.Back,
                    () => false
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
                        Common.Binders.ButtonClick(icd, engine,
                            imap.GetComponent<Button>("view_details"),
                            state.ItemDetails.Scene.Open,
                            () => item.Id
                        );
                        Common.Binders.Click(icd, engine,
                            imap.GetComponent<Common.Clickable>("select"),
                            () => SelectedItem.Write(item)
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
            public MyScene(IEngine engine) : base(engine, "Inventory")
            {
            }

            protected override IEnumerator<float> Preload()
            {
                for (int i = 0, n = 20; i < n; ++i)
                {
                    yield return i * 1f / n;
                }
            }
        }
    }
}