using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Writership;

namespace Examples.Scenes
{
    public class SceneStack
    {
        public readonly MultiOp<Scene> Register;
        public readonly MultiOp<Scene> Unregister;
        public readonly Li<Scene> ActiveScenes;
        public readonly Op<bool> Back;

        private readonly Li<Scene> registeredScenes;

        public SceneStack(IEngine engine)
        {
            Register = engine.MultiOp<Scene>(allowWriters: true);
            Unregister = engine.MultiOp<Scene>(allowWriters: true);
            ActiveScenes = engine.Li(new List<Scene>());
            Back = engine.Op<bool>(allowWriters: true);

            registeredScenes = engine.Li(new List<Scene>());
        }

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            engine.Worker(cd, Dep.On(Register, Unregister), () =>
            {
                var registered = registeredScenes.AsWriteProxy();
                for (int i = 0, n = Register.Count; i < n; ++i)
                {
                    registered.Add(Register[i]);
                }
                for (int i = 0, n = Unregister.Count; i < n; ++i)
                {
                    // TODO Should be RemoveExact
                    registered.Remove(Unregister[i]);
                }
                registered.Commit();
            });

            var watcher = engine.Wa(cd, registeredScenes, scene => scene.State);
            engine.Worker(cd, Dep.On(watcher, Unregister), () =>
            {
                var active = ActiveScenes.AsWriteProxy();
                for (int i = 0, n = Unregister.Count; i < n; ++i)
                {
                    active.Remove(Unregister[i]);
                }
                for (int i = 0, n = registeredScenes.Count; i < n; ++i)
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
                bool isSystem;
                if (!Back.TryRead(out isSystem) || ActiveScenes.Count <= 1) return;
                ActiveScenes[ActiveScenes.Count - 1].Back.Fire(isSystem);
            });
        }

        public void SetupUnity(CompositeDisposable cd, IEngine engine)
        {
            engine.Mainer(cd, Dep.On(Back), () =>
            {
                bool isSystem;
                if (!Back.TryRead(out isSystem) || !isSystem || ActiveScenes.Count > 1) return;

#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

            CoroutineExecutor.Instance.StartCoroutine(cd, CheckBack());
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