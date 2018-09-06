using System;
using System.Collections.Generic;
using Writership;

namespace Examples.TodoList
{
    public class State : IDisposable
    {
        public readonly IEl<int> NextId;
        public readonly IOp<string> CreateNewItem;
        public readonly ILi<TodoItem> Items;
        public readonly IOp<string> ToggleItemComplete;
        public readonly IEl<int> UncompletedCount;
        public readonly IOp<Empty> DeleteCompletedItems;
        public readonly IOp<string> DeleteItem;
        public readonly IOp<string> EditItem;
        public readonly IEl<string> EditingItemId;
        public readonly IOp<string> FinishEditItem;
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
            EditItem = engine.Op<string>();
            EditingItemId = engine.El<string>(null);
            FinishEditItem = engine.Op<string>();
            ItemFactory = new TodoItem.Factory(engine, ToggleItemComplete, EditingItemId, FinishEditItem);

            cd = new CompositeDisposable();

            cd.Add(engine.RegisterComputer(
                new object[] {
                    Items,
                    ToggleItemComplete.Applied,
                },
                () => Computers.UncompletedCount(
                    UncompletedCount,
                    Items
                )
            ));

            cd.Add(engine.RegisterComputer(
                new object[]
                {
                    CreateNewItem,
                    DeleteCompletedItems,
                    DeleteItem
                },
                () => Computers.Items(
                    Items,
                    NextId,
                    CreateNewItem,
                    DeleteCompletedItems,
                    DeleteItem,
                    ItemFactory
                )
            ));

            cd.Add(engine.RegisterComputer(
                new object[]
                {
                    EditItem,
                    FinishEditItem
                },
                () => Computers.EditingItemId(
                    EditingItemId,
                    EditItem,
                    FinishEditItem
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
        public readonly IEl<string> Content;
        public readonly IEl<bool> IsCompleted;

        private readonly CompositeDisposable cd;

        public TodoItem(
            IEngine engine,
            IOp<string> toggleComplete,
            IEl<string> editingItemId,
            IOp<string> finishEdit,
            string id,
            string content
        )
        {
            Id = id;
            Content = engine.El(content);
            IsCompleted = engine.El(false);

            cd = new CompositeDisposable();

            cd.Add(engine.RegisterComputer(
                new object[] {
                    toggleComplete
                },
                () => Computers.TodoItem.IsCompleted(
                    IsCompleted,
                    toggleComplete,
                    Id
                )
            ));


            cd.Add(engine.RegisterComputer(
                new object[] {
                    editingItemId,
                    finishEdit
                },
                () => Computers.TodoItem.Content(
                    Content,
                    editingItemId,
                    finishEdit,
                    Id
                )
            ));
        }

        public void Dispose()
        {
            cd.Dispose();
        }

        public class Factory
        {
            private readonly IEngine engine;
            private readonly IOp<string> toggleComplete;
            private readonly IEl<string> editingItemId;
            private readonly IOp<string> finishEdit;

            public Factory(
                IEngine engine,
                IOp<string> toggleComplete,
                IEl<string> editingItemId,
                IOp<string> finishEdit
            )
            {
                this.engine = engine;
                this.toggleComplete = toggleComplete;
                this.editingItemId = editingItemId;
                this.finishEdit = finishEdit;
            }

            public TodoItem Create(string id, string content)
            {
                return new TodoItem(engine, toggleComplete, editingItemId, finishEdit, id, content);
            }
        }
    }
}