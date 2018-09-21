using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Writership;

namespace Examples.Scenes
{
    public class CoroutineExecutor : MonoBehaviour
    {
        public static CoroutineExecutor Instance { get; private set; }

        public void Awake()
        {
            Instance = this;
        }
    }

    public class State
    {
        public readonly El<int> Gold;
        public readonly Home Home;
        public readonly Inventory Inventory;
        public readonly Op<Empty> Back;

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
        }

        public void SetupUnity(CompositeDisposable cd, IEngine engine)
        {
            engine.Mainer(cd, Dep.On(Home.Scene.Root), () =>
            {
                var root = Home.Scene.Root.Read();
                if (!root) return;
                var map = root.GetComponent<Common.Map>();
                var scd = root.GetComponent<Common.DisposeOnDestroy>().cd;

                Common.Binders.Label(scd, engine,
                    map.GetComponent<Text>("gold"), Gold,
                    i => string.Format("Gold: {0}", i)
                );
            });
        }
    }

    public class SceneStack
    {
        public readonly Li<Scene> Scenes;
        public readonly El<bool> IsBusy;

        public void Setup(CompositeDisposable cd, IEngine engine)
        {

        }
    }

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
                else throw new NotImplementedException();
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
            if (!open)
            {
                // TODO Trigger closing transition, wait for done
                UnityEngine.Object.Destroy(Root.Read());
                Root.Write(null);

                yield break;
            }

            var load = SceneManager.LoadSceneAsync(Name, Mode);
            //load.allowSceneActivation = false; // TODO Use this
            while (!load.isDone)
            {
                LoadingProgress.Write(load.progress);
                yield return null;
            }

            var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            var root = scene.GetRootGameObjects()[0];
            // TODO Trigger opening transition, wait for done
            Root.Write(root);

            // Watch
            while (root) yield return null;
            Root.Write(null);
        }
    }

    public class Home
    {
        public readonly Scene Scene;
        public readonly Op<Empty> OpenInventory;
    }

    public class Inventory
    {
        public readonly Scene Scene;
    }

    public class YesNoDialog
    {
        public readonly Scene Scene;
    }
}