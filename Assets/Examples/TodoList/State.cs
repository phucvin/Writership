using System;
using System.Collections.Generic;
using Writership;

namespace Examples.TodoList
{
    public class State : IDisposable
    {
        public readonly El<int> NextId;
        public readonly Op<string> CreateNewItem;
        public readonly Li<TodoItem> Items;
        public readonly Op<string> ToggleItemComplete;
        public readonly El<int> UncompletedCount;
        public readonly Op<Empty> DeleteCompletedItems;
        public readonly Op<string> DeleteItem;
        public readonly TodoItem.Factory ItemFactory;

        private readonly CompositeDisposable cd;

        public State(IEngine engine)
        {
            NextId = engine.El(1);
            CreateNewItem = engine.Op<string>();
            Items = engine.Li(new List<TodoItem>());
            ToggleItemComplete = engine.Op<string>();
            UncompletedCount = engine.El(0);
            DeleteCompletedItems = engine.Op<Empty>();
            DeleteItem = engine.Op<string>();
            ItemFactory = new TodoItem.Factory(engine, ToggleItemComplete);

            cd = new CompositeDisposable();

            cd.Add(engine.RegisterComputer(
                new object[] {
                    Items,
                    ToggleItemComplete.Applied,
                },
                () => UncompletedCount.Write(Computers.UncompletedCount(
                    Items.Read()
                ))
            ));

            cd.Add(engine.RegisterComputer(
                new object[]
                {
                    CreateNewItem,
                    DeleteCompletedItems,
                    DeleteItem
                },
                () => Computers.Items(
                    Items.AsWrite(),
                    NextId,
                    CreateNewItem.Read(),
                    DeleteCompletedItems.Read(),
                    DeleteItem.Read(),
                    ItemFactory
                )
            ));
        }

        public void Dispose()
        {
            cd.Dispose();
        }
    }

    public class TodoItem : IDisposable
    {
        public readonly string Id;
        public readonly El<string> Content;
        public readonly El<bool> IsCompleted;

        private readonly CompositeDisposable cd;

        public TodoItem(IEngine engine, Op<string> toggleComplete, string id, string content)
        {
            Id = id;
            Content = engine.El(content);
            IsCompleted = engine.El(false);

            cd = new CompositeDisposable();

            cd.Add(engine.RegisterComputer(
                new object[] {
                    toggleComplete
                },
                () => IsCompleted.Write(Computers.TodoItem.IsCompleted(
                    IsCompleted.Read(),
                    toggleComplete.Read(),
                    Id
                ))
            ));
        }

        public void Dispose()
        {
            cd.Dispose();
        }

        public class Factory
        {
            private readonly IEngine engine;
            private readonly Op<string> toggleComplete;

            public Factory(IEngine engine, Op<string> toggleComplete)
            {
                this.engine = engine;
                this.toggleComplete = toggleComplete;
            }

            public TodoItem Create(string id, string content)
            {
                return new TodoItem(engine, toggleComplete, id, content);
            }
        }
    }
}