using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Writership;

namespace Examples.Scenes
{
    public class ScenesMain : MonoBehaviour, IDisposable
    {
        public static ScenesMain Instance { get; private set; }

        [SerializeField]
        private Common.Map map = null;

        private IEngine engine;
        private State state;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public void Awake()
        {
            Instance = this;
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

            Common.Binders.ButtonClick(cd, engine,
                map.GetComponent<Button>("back"), state.SceneStack.Back,
                () => false
            );
            Common.Binders.Enabled(cd, engine,
                map.Get("back"), state.ShouldShowBack
            );

            // Test
            state.Home.Scene.Open.Fire(Empty.Instance);
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

        public void Back(bool isSystem)
        {
            state.SceneStack.Back.Fire(isSystem);
        }
    }
}