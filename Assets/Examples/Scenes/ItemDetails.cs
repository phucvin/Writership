using UnityEngine.UI;
using Writership;

namespace Examples.Scenes
{
    public class ItemDetails
    {
        public readonly Scene<string> Scene;
        public readonly El<Item> Item;
        
        public ItemDetails(IEngine engine)
        {
            Scene = new Scene<string>(engine, "ItemDetails");
            Item = engine.El<Item>(null);
        }

        public void Setup(CompositeDisposable cd, IEngine engine, State state)
        {
            Scene.Setup(cd, engine);

            engine.Worker(cd, Dep.On(Scene.Open, state.Inventory.Items), () =>
            {
                var item = Item.Read();
                var items = state.Inventory.Items.Read();

                if (Scene.Open)
                {
                    var id = Scene.Open.First;
                    for (int i = 0, n = items.Count; i < n; ++i)
                    {
                        if (items[i].Id == id)
                        {
                            Item.Write(items[i]);
                            return;
                        }
                    }
                }
                if (item != null && !items.Contains(Item))
                {
                    Item.Write(null);
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
                if (!root) return;
                var map = root.GetComponent<Common.Map>();
                var scd = root.GetComponent<Common.DisposeOnDestroy>().cd;

                Common.Binders.Label(scd, engine,
                    map.GetComponent<Text>("name"), Item,
                    item => item.Name
                );
            });
        }
    }
}