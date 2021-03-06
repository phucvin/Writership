﻿using UnityEngine.UI;
using Writership;

namespace Examples.Scenes
{
    public class ItemDetails
    {
        public static void CreateAndShow(IEngine engine, State state, Item item)
        {
            var details = new ItemDetails(engine);
            var cd = new CompositeDisposable();
            details.Setup(cd, engine, state);
            details.SetupUnity(cd, engine, state);
            details.Scene.SetupDisposeOnClose(cd, engine);
            details.Scene.Open.Fire(item);
        }

        public readonly Scene<Item> Scene;
        public readonly El<Item> Item;
        
        public ItemDetails(IEngine engine)
        {
            Scene = new Scene<Item>(engine, "ItemDetails");
            Item = engine.El<Item>(null);
        }

        public void Setup(CompositeDisposable cd, IEngine engine, State state)
        {
            Scene.Setup(cd, engine, state.SceneStack);

            engine.Worker(cd, Dep.On(Scene.Open, state.Inventory.Items), () =>
            {
                Item openItem;
                if (Scene.Open.TryRead(out openItem))
                {
                    Item.Write(openItem);
                }
                else
                {
                    var item = Item.Read();
                    var items = state.Inventory.Items.Read();
                    if (item != null && !items.Contains(item))
                    {
                        Item.Write(null);
                    }
                }
            });
            engine.Worker(cd, Dep.On(Scene.State, Item), () =>
            {
                if (Scene.State == SceneState.Opened && Item.Read() == null)
                {
                    Scene.Close.Fire(Empty.Instance);
                }
            });
        }

        public void SetupUnity(CompositeDisposable cd, IEngine engine, State state)
        {
            Scene.SetupUnity(cd, engine);

            engine.Mainer(cd, Dep.On(Scene.Root), () =>
            {
                var root = Scene.Root.Read();
                var item = Item.Read();
                if (!root || item == null) return;
                var map = root.GetComponent<Common.Map>();
                var scd = root.GetComponent<Common.DisposeOnDestroy>().cd;

                Common.Binders.Label(scd, engine,
                    map.GetComponent<Text>("name"), item.Name
                );
                Common.Binders.Image(scd, engine,
                    map.GetComponent<Image>("image"), item.Image
                );
                Common.Binders.Label(scd, engine,
                    map.GetComponent<Text>("level"), item.Level,
                    i => string.Format("Level {0}", i)
                );
                Common.Binders.ButtonClick(scd, engine,
                    map.GetComponent<Button>("sell"), state.Inventory.SellItem.Trigger,
                    () => item
                );
            });
        }
    }
}