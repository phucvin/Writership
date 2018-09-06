using System;
using System.Collections;
using UnityEngine;
using Writership;

namespace Examples.TodoList
{
    public class TodoListMain : MonoBehaviour, IDisposable
    {
        private IEngine engine;
        private State state;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public void Setup()
        {
            Dispose();

            cd.Add(engine = new MultithreadEngine());
            cd.Add(state = new State(engine));
        }

        public void Dispose()
        {
            cd.Dispose();
        }

        public IEnumerator Start()
        {
            Setup();

            engine.RegisterListener(
                new object[] {
                    state.Items,
                    state.ToggleItemComplete
                },
                () =>
                {
                    var items = state.Items.Read();
                    Debug.Log("Items begin");
                    for (int i = 0, n = items.Count; i < n; ++i)
                    {
                        var item = items[i];
                        Debug.Log(item.Id + " | " + item.Content.Read() + " | " + (item.IsCompleted.Read() ? "x" : "-"));
                    }
                    Debug.Log("Items end");
                }
            );

            yield return null;
            state.CreateNewItem.Fire("hello world");
            yield return null;
            state.ToggleItemComplete.Fire("1");
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