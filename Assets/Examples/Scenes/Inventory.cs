using UnityEngine.UI;
using Writership;

namespace Examples.Scenes
{
    public class Inventory
    {
        public readonly Scene Scene;
        public readonly Op<Empty> UpgradeItem;

        public void Setup(CompositeDisposable cd, IEngine engine, State state)
        {
            Scene.Setup(cd, engine);
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

                Common.Binders.ButtonClick(scd, engine,
                    map.GetComponent<Button>("confirm"), UpgradeItem,
                    () => Empty.Instance
                );
            });
        }
    }
}