using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Writership;

namespace Examples.Scenes
{
    public class Home
    {
        public readonly Scene<Empty> Scene;
        public readonly ElWithRaw<string, string> SelectedTab;

        public Home(IEngine engine)
        {
            Scene = new Scene<Empty>(engine, "Home", LoadSceneMode.Single);
            SelectedTab = engine.ElWithRaw("tab_base");
        }

        public void Setup(CompositeDisposable cd, IEngine engine, State state)
        {
            Scene.Setup(cd, engine, state.SceneStack);

            engine.Worker(cd, Dep.On(SelectedTab.Raw, state.Inventory.Scene.Open), () =>
            {
                if (state.Inventory.Scene.Open)
                {
                    SelectedTab.Write("tab_more");
                }
                else SelectedTab.Write(SelectedTab.Raw);
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
                    map.GetComponent<Text>("gold"), state.Gold,
                    i => string.Format("Gold: {0}", i)
                );
                Common.Binders.ButtonClick(scd, engine,
                    map.GetComponent<Button>("inventory"), state.Inventory.Scene.Open,
                    () => Empty.Instance
                );
                Common.Binders.Tabs(scd, engine, map,
                    new string[] { "tab_base", "tab_more" },
                    new string[] { "toggle_base", "toggle_more" },
                    SelectedTab.Raw, s => s,
                    SelectedTab, s => s
                );
            });
        }
    }
}