using Writership;

namespace Examples.Scenes
{
    public class State
    {
        public readonly El<int> Gold;
        public readonly Home Home;
        public readonly Inventory Inventory;

        public State(IEngine engine)
        {
            Gold = engine.El(100);
            Home = new Home(engine);
            Inventory = new Inventory(engine);
        }

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            var state = this;

            Home.Setup(cd, engine, state);
            Inventory.Setup(cd, engine, state);

            Home.Scene.Open.Fire(Empty.Instance);
        }

        public void SetupUnity(CompositeDisposable cd, IEngine engine)
        {
            var state = this;

            Home.SetupUnity(cd, engine, state);
            Inventory.SetupUnity(cd, engine, state);
        }
    }
}