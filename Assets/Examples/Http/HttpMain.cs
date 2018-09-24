using System;
using UnityEngine;
using UnityEngine.UI;
using Writership;

namespace Examples.Http
{
    public class HttpMain : MonoBehaviour, IDisposable
    {
        public static HttpMain Instance { get; private set; }

        [SerializeField]
        private Common.Map map = null;

        private IEngine engine;
        private State state;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public void Awake()
        {
            Instance = this;
        }

        public void Setup()
        {
            engine = new SinglethreadEngine();
            state = new State(engine);

            state.Setup(cd, engine);

            Common.Binders.Label(cd, engine,
                map.GetComponent<Text>("user_id"), state.UserId,
                i => string.Format("User ID: {0}", i.HasValue ? i.Value.ToString() : "<none>")
            );
            Common.Binders.Label(cd, engine,
                map.GetComponent<Text>("repo_count"), state.RepoCount,
                i => string.Format("{0} repositories", i.HasValue ? i.Value.ToString() : "<none>")
            );
            Common.Binders.InputField(cd, engine,
                map.GetComponent<InputField>("user_name"), state.UserName,
                s => s
            );
            Common.Binders.Enabled(cd, engine,
                map.Get("is_busy"), state.IsBusy
            );

            engine.Mainer(cd, Dep.On(state.HttpUserId.Error), () =>
            {
                HttpError error;
                if (state.HttpUserId.Error.TryRead(out error))
                {
                    Debug.Log(error);
                }
            });
        }

        public void Dispose()
        {
            cd.Dispose();
            engine.Dispose();
        }

        public void Start()
        {
            Setup();
        }

        public void OnDestroy()
        {
            Dispose();
        }

        public void Update()
        {
            engine.Update();
        }
    }
}