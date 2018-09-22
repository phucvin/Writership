using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Writership;

namespace Examples.Scenes
{
    public enum SceneState
    {
        Closed,
        Opening,
        Opened,
        Closing
    }

    public class Scene
    {
        public readonly string Name;
        public readonly LoadSceneMode Mode;
        public readonly El<SceneState> State;
        public readonly El<GameObject> Root;
        public readonly El<float> LoadingProgress;
        public readonly Op<Empty> Open;
        public readonly Op<Empty> Close;

        public Scene(IEngine engine, string name, LoadSceneMode mode = LoadSceneMode.Additive)
        {
            Name = name;
            Mode = mode;
            State = engine.El(SceneState.Closed);
            Root = engine.El<GameObject>(null);
            LoadingProgress = engine.El(0f);
            Open = engine.Op<Empty>();
            Close = engine.Op<Empty>();
        }

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
        }

        public void SetupUnity(CompositeDisposable cd, IEngine engine)
        {
            engine.Mainer(cd, Dep.On(Open, Close, Root), () =>
            {
                if (State == SceneState.Opening && Root.Read())
                {
                    State.Write(SceneState.Opened);
                }
                else if (State == SceneState.Closing && !Root.Read())
                {
                    State.Write(SceneState.Closed);
                }
                else if (State == SceneState.Closed && Open)
                {
                    State.Write(SceneState.Opening);
                }
                else if (State == SceneState.Opened && Close)
                {
                    State.Write(SceneState.Closing);
                }
                else if (Open || Close)
                {
                    throw new NotImplementedException();
                }
            });
            engine.Mainer(cd, Dep.On(State), () =>
            {
                if (State == SceneState.Opening)
                {
                    CoroutineExecutor.Instance.StartCoroutine(Exec(true));
                }
                else if (State == SceneState.Closing)
                {
                    CoroutineExecutor.Instance.StartCoroutine(Exec(false));
                }
            });
        }

        private IEnumerator Exec(bool open)
        {
            GameObject root;
            Transitioner transitioner;

            if (!open)
            {
                root = Root.Read();
                transitioner = root.GetComponent<Transitioner>();
                if (transitioner) yield return transitioner.Out();
                yield return SceneManager.UnloadSceneAsync(Root.Read().scene);
                yield break;
            }

            var load = SceneManager.LoadSceneAsync(Name, Mode);
            //load.allowSceneActivation = false; // TODO Use this
            while (!load.isDone)
            {
                LoadingProgress.Write(load.progress);
                yield return null;
            }
            
            var scene = SceneManager.GetSceneByName(Name);
            root = scene.GetRootGameObjects()[0];
            transitioner = root.GetComponent<Transitioner>();
            if (transitioner) yield return transitioner.In();
            Root.Write(root);

            // Watch
            while (root) yield return null;
            Root.Write(null);
        }
    }
}