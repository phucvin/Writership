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
            Dispose();

            cd.Add(engine = new MultithreadEngine());
            cd.Add(state = new State(engine));

            cd.Add(Common.Binders.ButtonClick(engine, map.GetComponent<Button>("newItem"),
                state.CreateNewItem, () => map.GetComponent<InputField>("newItemContent").text
            ));
            cd.Add(Common.Binders.Label(engine, map.GetComponent<Text>("uncompletedCount"),
                state.UncompletedCount, i => string.Format("Uncompleted count: {0}", i)
            ));
            cd.Add(Common.Binders.List(engine,
                map.GetComponent<Transform>("itemsParent"),
                map.GetComponent<Common.Map>("itemPrefab"),
                state.Items, (map, item) =>
                {
                    var cd = new CompositeDisposable();
                    cd.Add(Common.Binders.Label(engine, map.GetComponent<Text>("content"),
                        item.Content, s => s
                    ));
                    return cd;
                }
            ));
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