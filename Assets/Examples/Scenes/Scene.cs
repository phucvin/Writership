using System;
using System.Collections;
using System.Collections.Generic;
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

    public abstract class Scene
    {
        public readonly string Name;
        public readonly LoadSceneMode Mode;
        public readonly bool BackAutoClose;
        public readonly El<SceneState> State;
        public readonly El<GameObject> Root;
        public readonly El<float> LoadingProgress;
        public readonly Op<Empty> Close;
        public readonly Op<bool> Back;

        protected Scene(IEngine engine, string name, LoadSceneMode mode, bool backAutoClose)
        {
            Name = name;
            Mode = mode;
            BackAutoClose = backAutoClose;
            State = engine.El(SceneState.Closed);
            Root = engine.El<GameObject>(null);
            LoadingProgress = engine.El(1f);
            Close = engine.Op<Empty>(allowWriters: true);
            Back = engine.Op<bool>(allowWriters: true);
        }
    }

    public class Scene<T> : Scene
    {
        public readonly Op<T> Open;

        public Scene(IEngine engine, string name, LoadSceneMode mode = LoadSceneMode.Additive,
            bool backAutoClose = true)
            : base(engine, name, mode, backAutoClose)
        {
            Open = engine.Op<T>(allowWriters: true);
        }

        public void Setup(CompositeDisposable cd, IEngine engine, SceneStack stack)
        {
            if (stack != null)
            {
                stack.Register.Fire(this);
                cd.Add(new DisposableAction(() => stack.Unregister.Fire(this)));
            }
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
                else if (!Root.Read() &&
                    (State == SceneState.Closing || State == SceneState.Opened))
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
                    Common.CoroutineExecutor.Instance.StartCoroutine(cd, Exec(true));
                }
                else if (State == SceneState.Closing)
                {
                    Common.CoroutineExecutor.Instance.StartCoroutine(cd, Exec(false));
                }
            });
        }

        public void SetupDisposeOnClose(CompositeDisposable cd, IEngine engine)
        {
            engine.Mainer(cd, Dep.On(Root), () =>
            {
                if (!Root.Read() &&
                    (State == SceneState.Closing || State == SceneState.Opened))
                {
                    cd.Dispose();
                }
            });
        }

        protected virtual IEnumerator<float> Preload()
        {
            return null;
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

            var preload = Preload();
            while (preload != null && preload.MoveNext())
            {
                LoadingProgress.Write(preload.Current / 2f);
                yield return null;
            }

            var load = SceneManager.LoadSceneAsync(Name, Mode);
            //load.allowSceneActivation = false; // TODO Use this
            while (!load.isDone)
            {
                LoadingProgress.Write(0.5f + (load.progress / 2f));
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