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
                
                Common.Binders.ButtonClick(scd, engine,
                    map.GetComponent<Button>("close"), Scene.Close,
                    () => Empty.Instance
                );
                Common.Binders.ButtonClick(scd, engine,
                    map.GetComponent<Button>("upgrade"), UpgradeItem.Trigger,
                    () => "Sword"
                );
            });
        }
    }
}