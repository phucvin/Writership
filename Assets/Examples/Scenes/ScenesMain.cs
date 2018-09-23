﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Writership;

namespace Examples.Scenes
{
    public class ScenesMain : MonoBehaviour, IDisposable
    {
        [SerializeField]
        private Common.Map map = null;

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

            Common.Binders.ButtonClick(cd, engine,
                map.GetComponent<Button>("back"), state.SceneStack.Back,
                () => false
            );
            Common.Binders.Enabled(cd, engine,
                state.ShouldShowBack, map.Get("back")
            );

            // Test
            state.Home.Scene.Open.Fire(Empty.Instance);
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