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
        public readonly Op<string> EditItem;
        public readonly El<string> EditingItemId;
        public readonly Op<string> FinishEditItem;
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
                // TODO Fix unnecessary mark dirty if no change
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
                // TODO Fix unnecessary mark dirty if no change
                () => Computers.Items(
                    Items.AsWrite(),
                    NextId,
                    CreateNewItem.Read(),
                    DeleteCompletedItems.Read(),
                    DeleteItem.Read(),
                    ItemFactory
                )
            ));

            cd.Add(engine.RegisterComputer(
                new object[]
                {
                    EditItem,
                    FinishEditItem
                },
                // TODO Fix unnecessary mark dirty if no change
                () => EditingItemId.Write(Computers.EditingItem(
                    EditItem.Read(),
                    FinishEditItem.Read()
                ))
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

        public TodoItem(
            IEngine engine,
            Op<string> toggleComplete,
            El<string> editingItemId,
            Op<string> finishEdit,
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
                // TODO Fix unnecessary mark dirty if no change
                () => IsCompleted.Write(Computers.TodoItem.IsCompleted(
                    IsCompleted.Read(),
                    toggleComplete.Read(),
                    Id
                ))
            ));


            cd.Add(engine.RegisterComputer(
                new object[] {
                    editingItemId,
                    finishEdit
                },
                // TODO Fix unnecessary mark dirty if no change
                () => Content.Write(Computers.TodoItem.Content(
                    Content.Read(),
                    editingItemId.Read(),
                    finishEdit.Read(),
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
            private readonly El<string> editingItemId;
            private readonly Op<string> finishEdit;

            public Factory(
                IEngine engine,
                Op<string> toggleComplete,
                El<string> editingItemId,
                Op<string> finishEdit
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