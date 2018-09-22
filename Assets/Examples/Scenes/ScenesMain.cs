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
            state.Inventory.UpgradeItem.Status.Write(true);
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