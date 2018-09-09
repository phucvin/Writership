using System;
using UnityEngine;
using UnityEngine.UI;
using Writership;

namespace Examples.TodoList
{
    public class TodoListMain : MonoBehaviour, IDisposable
    {
        [SerializeField]
        private Common.Map map = null;

        private IEngine engine;
        private State state;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public void Setup()
        {
            engine = new MultithreadEngine();
            state = new State(cd, engine);

            Common.Binders.ButtonClick(cd, engine,
                map.GetComponent<Button>("newItem"), state.CreateNewItem,
                () => map.GetComponent<InputField>("newItemContent").text
            );
            Common.Binders.Label(cd, engine,
                map.GetComponent<Text>("uncompletedCount"), state.UncompletedCount,
                i => string.Format("Uncompleted count: {0}", i)
            );
            Common.Binders.List(cd, engine,
                map.GetComponent<Transform>("itemsParent"),
                map.GetComponent<Common.Map>("itemPrefab"),
                state.Items,
                (cd, map, item) =>
                {
                    Common.Binders.Label(cd, engine, map.GetComponent<Text>("content"),
                        item.Content, s => s
                    );
                }
            );
        }

        public void Dispose()
        {
            cd.Dispose();
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