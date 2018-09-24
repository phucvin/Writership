﻿using System.Collections.Generic;
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
        public readonly VerifyConfirmOp<string> UpgradeItem;

        public Inventory(IEngine engine)
        {
            Scene = new MyScene(engine);
            Items = engine.Li(new List<Item>());
            ItemFactory = new Item.Factory();
            SelectedItem = engine.El<Item>(null);
            UpgradeItem = new VerifyConfirmOp<string>(engine,
                s => string.Format("Do you want to upgrade {0}?", s)
            );
        }

        public void Setup(CompositeDisposable cd, IEngine engine, State state)
        {
            Scene.Setup(cd, engine);
            UpgradeItem.Setup(cd, engine);
            ItemFactory.Setup(cd, engine, state);

            engine.Worker(cd, Dep.On(state.Gold, SelectedItem), () =>
            {
                UpgradeItem.Status.Write(state.Gold >= 10 && SelectedItem.Read() != null);
            });
            engine.Worker(cd, Dep.On(Scene.Open), () =>
            {
                var items = Items.AsWriteProxy();
                if (Scene.Open)
                {
                    items.Add(ItemFactory.Create(
                        items.Count.ToString(),
                        "Sword " + items.Count,
                        "item_" + items.Count,
                        1
                    ));
                }
                items.Commit();
            });
            engine.Worker(cd, Dep.On(UpgradeItem.Dialog.Back), () =>
            {
                var back = UpgradeItem.Dialog.Back;
                if (!back || !back.First) return;
                UpgradeItem.Dialog.Close.Fire(Empty.Instance);
            });
        }

        public void SetupUnity(CompositeDisposable cd, IEngine engine, State state)
        {
            Scene.SetupUnity(cd, engine);
            UpgradeItem.SetupUnity(cd, engine);

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
                    () => SelectedItem.Read().Id
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