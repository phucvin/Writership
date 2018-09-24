using Writership;

namespace Examples.Scenes
{
    public class Item
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string Image;
        public readonly El<int> Level;

        public Item(IEngine engine, string id, string name, string image, int level)
        {
            Id = id;
            Name = name;
            Image = image;
            Level = engine.El(level);
        }

        public void Setup(CompositeDisposable cd, IEngine engine, State state)
        {
            engine.OpWorker(cd, Dep.On(state.Inventory.UpgradeItem.Yes), () =>
            {
                if (state.Inventory.UpgradeItem.Yes.First == Id)
                {
                    Level.Write(Level + 1);
                }
            });
        }

        public class Factory : CompositeDisposableFactory<Item>
        {
            private IEngine engine;
            private State state;

            public void Setup(CompositeDisposable cd, IEngine engine, State state)
            {
                this.engine = engine;
                this.state = state;
                cd.Add(this);
            }

            public Item Create(string id, string name, string image, int level)
            {
                var item = new Item(engine, id, name, image, level);
                var cd = Add(item);
                item.Setup(cd, engine, state);
                return item;
            }

            public void Dispose(Item item)
            {
                Remove(item).Dispose();
            }
        }
    }
}