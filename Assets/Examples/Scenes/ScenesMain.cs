using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Writership;

namespace Examples.Scenes
{
    public class ScenesMain : MonoBehaviour, IDisposable
    {
        private IEngine engine;
        private State state;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void OnDestroy()
        {
            Dispose();
        }

        public void Setup()
        {
            engine = new SinglethreadEngine();
            state = new State(engine);

            state.Setup(cd, engine);
            state.SetupUnity(cd, engine);

            // Test
            state.Home.Scene.Open.Fire(Empty.Instance);
            StartCoroutine(testUpgradeItemStatus());
            engine.Mainer(cd, Dep.On(state.Inventory.UpgradeItem.Yes), () =>
            {
                if (!state.Inventory.UpgradeItem.Yes) return;
                Debug.LogFormat("UpgradeItem {0}, Yes",
                    state.Inventory.UpgradeItem.Yes.First);
            });
            engine.Mainer(cd, Dep.On(state.Inventory.UpgradeItem.Rejected), () =>
            {
                if (!state.Inventory.UpgradeItem.Rejected) return;
                Debug.LogFormat("UpgradeItem {0}, Rejected",
                    state.Inventory.UpgradeItem.Rejected.First);
            });
        }

        private IEnumerator testUpgradeItemStatus()
        {
            bool b = true;
            while (true)
            {
                state.Inventory.UpgradeItem.Status.Write(b);
                b = !b;
                yield return new WaitForSeconds(3f);
            }
        }

        public void Dispose()
        {
            cd.Dispose();
            engine.Dispose();
        }

        public IEnumerator Start()
        {
            var activeScene = SceneManager.GetActiveScene();
            for (int i = 0, n = SceneManager.sceneCount; i < n; ++i)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene == activeScene) continue;
                yield return SceneManager.UnloadSceneAsync(scene);
                --i;
                --n;
            }

            Setup();
            while (true)
            {
                engine.Update();
                yield return null;
            }
        }
    }
}