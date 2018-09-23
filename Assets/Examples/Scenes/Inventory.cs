using System.Collections.Generic;
using UnityEngine.UI;
using Writership;

namespace Examples.Scenes
{
    public class Inventory
    {
        public readonly Scene<int> Scene;
        public readonly VerifyConfirmOp<string> UpgradeItem;

        public Inventory(IEngine engine)
        {
            Scene = new MyScene(engine);
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
                Common.Binders.Click(scd, engine,
                    map.GetComponent<Common.Clickable>("back"), Scene.Back,
                    () => false
                );
                Common.Binders.Click(scd, engine,
                    map.GetComponent<Common.Clickable>("upgrade"), UpgradeItem.Trigger,
                    () => "Sword"
                );
                Common.Binders.ButtonInteractable(scd, engine,
                    map.GetComponent<Button>("upgrade"), UpgradeItem.Status,
                    b => b
                );
            });
        }

        private class MyScene : Scene<int>
        {
            public MyScene(IEngine engine) : base(engine, "Inventory")
            {
            }

            protected override IEnumerator<float> Preload()
            {
                for (int i = 0, n = 100; i < n; ++i)
                {
                    yield return i * 1f / n;
                }
            }
        }
    }
}