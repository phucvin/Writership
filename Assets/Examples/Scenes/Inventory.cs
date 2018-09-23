using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Writership;

namespace Examples.Scenes
{
    public class Inventory
    {
        public readonly Scene Scene;
        public readonly VerifyConfirmOp<string> UpgradeItem;

        public Inventory(IEngine engine)
        {
            Scene = new Scene(engine, "Inventory", LoadSceneMode.Additive);
            UpgradeItem = new VerifyConfirmOp<string>(engine,
                s => string.Format("Do you want to upgrade {0}?", s)
            );
        }

        public void Setup(CompositeDisposable cd, IEngine engine, State state)
        {
            Scene.Setup(cd, engine);
            UpgradeItem.Setup(cd, engine);

            engine.Worker(cd, Dep.On(state.Gold), () =>
            {
                UpgradeItem.Status.Write(state.Gold >= 10);
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
                Common.Binders.ButtonClick(scd, engine,
                    map.GetComponent<Button>("back"), Scene.Back,
                    () => false
                );
                Common.Binders.ButtonClick(scd, engine,
                    map.GetComponent<Button>("upgrade"), UpgradeItem.Trigger,
                    () => "Sword"
                );
                Common.Binders.TextColor(scd, engine,
                    map.GetComponent<Text>("upgrade_text"), UpgradeItem.Status,
                    // TODO Read from (global) map too
                    ok => ok ? Color.black : Color.gray
                );
            });
        }
    }
}