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
        public readonly SceneStack SceneStack;
        public readonly El<bool> ShouldShowBack;

        public State(IEngine engine)
        {
            Gold = engine.El(100);
            Home = new Home(engine);
            Inventory = new Inventory(engine);
            SimpleLoading = new Scene(engine, "SimpleLoading", LoadSceneMode.Additive);
            SceneStack = new SceneStack(engine);
            ShouldShowBack = engine.El(false);
        }

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            var state = this;

            Home.Setup(cd, engine, state);
            Inventory.Setup(cd, engine, state);
            SimpleLoading.Setup(cd, engine);

            SceneStack.Register(Home.Scene);
            SceneStack.Register(Inventory.Scene);
            SceneStack.Register(Inventory.UpgradeItem.Dialog);
            SceneStack.Setup(cd, engine);
            
            engine.Worker(cd, Dep.On(SimpleLoading.State,
                Home.Scene.State, Inventory.Scene.State,
                Inventory.UpgradeItem.Dialog.State), () =>
            {
                bool needLoading = Home.Scene.State == SceneState.Opening ||
                    Inventory.Scene.State == SceneState.Opening ||
                    Inventory.UpgradeItem.Dialog.State == SceneState.Opening;

                if (SimpleLoading.State == SceneState.Closed && needLoading)
                {
                    SimpleLoading.Open.Fire(Empty.Instance);
                }
                else if (SimpleLoading.State == SceneState.Opened && !needLoading)
                {
                    SimpleLoading.Close.Fire(Empty.Instance);
                }
            });
            engine.Worker(cd, Dep.On(SceneStack.ActiveScenes), () =>
            {
                var active = SceneStack.ActiveScenes.Read();
                ShouldShowBack.Write(active.Count > 1 && active[active.Count - 1].BackAutoClose);
            });
        }

        public void SetupUnity(CompositeDisposable cd, IEngine engine)
        {
            var state = this;

            Home.SetupUnity(cd, engine, state);
            Inventory.SetupUnity(cd, engine, state);
            SimpleLoading.SetupUnity(cd, engine);
            SceneStack.SetupUnity(cd, engine);
        }
    }
}