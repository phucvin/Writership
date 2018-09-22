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
        public readonly bool BackAutoClose;
        public readonly El<SceneState> State;
        public readonly El<GameObject> Root;
        public readonly El<float> LoadingProgress;
        public readonly Op<Empty> Open;
        public readonly Op<Empty> Close;
        public readonly Op<bool> Back;

        public Scene(IEngine engine, string name, LoadSceneMode mode = LoadSceneMode.Additive,
            bool backAutoClose = true)
        {
            Name = name;
            Mode = mode;
            BackAutoClose = backAutoClose;
            State = engine.El(SceneState.Closed);
            Root = engine.El<GameObject>(null);
            LoadingProgress = engine.El(1f);
            Open = engine.Op<Empty>(allowWriters: true);
            Close = engine.Op<Empty>(allowWriters: true);
            Back = engine.Op<bool>(allowWriters: true);
        }

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            if (BackAutoClose)
            {
                engine.Worker(cd, Dep.On(Back), () =>
                {
                    if (Back) Close.Fire(Empty.Instance);
                });
            }
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
                else if (State == SceneState.Opened && !Root.Read())
                {
                    State.Write(SceneState.Closed);
                }
                else if (State == SceneState.Closing && Close)
                {
                    // Ignore
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
                    CoroutineExecutor.Instance.StartCoroutine(cd, Exec(true));
                }
                else if (State == SceneState.Closing)
                {
                    CoroutineExecutor.Instance.StartCoroutine(cd, Exec(false));
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

            if (Name != "SimpleLoading" && UnityEngine.Random.value < 0.5f)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 3f));
            }

            var load = SceneManager.LoadSceneAsync(Name, Mode);
            //load.allowSceneActivation = false; // TODO Use this
            while (!load.isDone)
            {
                LoadingProgress.Write(load.progress);
                yield return null;
            }
            LoadingProgress.Write(1f);
            
            var scene = SceneManager.GetSceneByName(Name);
            root = scene.GetRootGameObjects()[0];
            transitioner = root.GetComponent<Transitioner>();
            if (transitioner) transitioner.In();
            Root.Write(root);

            // Watch
            while (root) yield return null;
            Root.Write(null);
        }
    }
}