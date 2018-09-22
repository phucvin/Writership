using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Writership;

namespace Examples.Scenes
{
    public class SceneStack
    {
        public readonly Li<Scene> ActiveScenes;
        public readonly Op<bool> Back;

        private readonly List<Scene> registeredScenes;

        public SceneStack(IEngine engine)
        {
            ActiveScenes = engine.Li(new List<Scene>());
            Back = engine.Op<bool>(allowWriters: true);

            registeredScenes = new List<Scene>();
        }

        public void Register(Scene scene)
        {
            registeredScenes.Add(scene);
        }

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            var states = new object[registeredScenes.Count];
            for (int i = 0, n = registeredScenes.Count; i < n; ++i)
            {
                states[i] = registeredScenes[i].State;
            }
            engine.Worker(cd, states, () =>
            {
                var active = ActiveScenes.AsWriteProxy();
                for (int i = 0, n = states.Length; i < n; ++i)
                {
                    var scene = registeredScenes[i];
                    var state = scene.State.Read();
                    if ((state == SceneState.Opened || state == SceneState.Opening) &&
                        !active.Contains(scene))
                    {
                        active.Add(scene);
                    }
                    else if (state == SceneState.Closed && active.Contains(scene))
                    {
                        active.Remove(scene);
                    }
                }
                active.Commit();
            });

            engine.Worker(cd, Dep.On(Back), () =>
            {
                if (!Back || ActiveScenes.Count <= 1) return;
                ActiveScenes[ActiveScenes.Count - 1].Back.Fire(Back.First);
            });
        }

        public void SetupUnity(CompositeDisposable cd, IEngine engine)
        {
            engine.Mainer(cd, Dep.On(Back), () =>
            {
                if (!Back || !Back.First || ActiveScenes.Count > 1) return;

#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

            // TODO Have to add coroutine to cd
            CoroutineExecutor.Instance.StartCoroutine(CheckBack());
        }

        private IEnumerator CheckBack()
        {
            while (true)
            {
                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    Back.Fire(true);
                }
                yield return null;
            }
        }
    }
}