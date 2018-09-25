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

            engine.Worker(cd, Dep.On(SelectedTab.Raw, Scene.Open), () =>
            {
                SelectedTab.Write(SelectedTab.Raw);
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

                string[] tabs = { "tab_base", "tab_more" };
                string[] toggles = { "toggle_base", "toggle_more" };
                engine.Reader(cd, Dep.On(SelectedTab), () =>
                {
                    string selected = SelectedTab.Read();
                    for (int i = 0, n = tabs.Length; i < n; ++i)
                    {
                        // TODO Fade animation
                        map.Get(tabs[i]).SetActive(tabs[i] == selected);
                    }
                });
                for (int i = 0, n = toggles.Length; i < n; ++i)
                {
                    string tab = tabs[i];
                    string toggle = toggles[i];
                    Common.Binders.ToggleIsOn(scd, engine,
                        map.GetComponent<Toggle>(toggles[i]), SelectedTab,
                        selected => selected == tab
                    );
                    Common.Binders.ToggleChange(scd, engine,
                        map.GetComponent<Toggle>(toggles[i]),
                        b => { if (b) SelectedTab.Raw.Write(tab); }
                    );
                }
            });
        }
    }
}