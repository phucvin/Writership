using UnityEngine.SceneManagement;
using Writership;

namespace Examples.Scenes
{
    public class State
    {
        public readonly El<int> Gold;
        public readonly Home Home;
        public readonly Inventory Inventory;
        public readonly Scene SimpleLoading;

        public State(IEngine engine)
        {
            Gold = engine.El(100);
            Home = new Home(engine);
            Inventory = new Inventory(engine);
            SimpleLoading = new Scene(engine, "SimpleLoading", LoadSceneMode.Additive);
        }

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            var state = this;

            Home.Setup(cd, engine, state);
            Inventory.Setup(cd, engine, state);
            SimpleLoading.Setup(cd, engine);

            engine.Worker(cd, Dep.On(SimpleLoading.State, Home.Scene.State, Inventory.Scene.State), () =>
            {
                bool needLoading = Home.Scene.State == SceneState.Opening ||
                    Inventory.Scene.State == SceneState.Opening;
                if (SimpleLoading.State == SceneState.Closed && needLoading)
                {
                    SimpleLoading.Open.Fire(Empty.Instance);
                }
                else if (SimpleLoading.State == SceneState.Opened && !needLoading)
                {
                    SimpleLoading.Close.Fire(Empty.Instance);
                }
            });

            Home.Scene.Open.Fire(Empty.Instance);
        }

        public void SetupUnity(CompositeDisposable cd, IEngine engine)
        {
            var state = this;

            Home.SetupUnity(cd, engine, state);
            Inventory.SetupUnity(cd, engine, state);
            SimpleLoading.SetupUnity(cd, engine);
        }
    }
}