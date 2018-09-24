using System.Collections.Generic;
using Writership;

namespace Examples.TodoList
{
    public class State
    {
        public readonly IEl<int> NextId;
        public readonly IMultiOp<string> CreateNewItem;
        public readonly ILi<ITodoItem> Items;
        public readonly IMultiOp<string> ToggleItemComplete;
        public readonly IEl<int> UncompletedCount;
        public readonly IMultiOp<Empty> DeleteCompletedItems;
        public readonly IMultiOp<string> DeleteItem;
        public readonly IMultiOp<string> EditItem;
        public readonly IEl<string> EditingItemId;
        public readonly IMultiOp<string> FinishEditItem;
        public readonly ITodoItemFactory ItemFactory;

        public State(CompositeDisposable cd, IEngine engine)
        {
            NextId = engine.El(1);
            CreateNewItem = engine.MultiOp<string>();
            Items = engine.Li(new List<ITodoItem>());
            ToggleItemComplete = engine.MultiOp<string>(needApplied: true);
            UncompletedCount = engine.El(0);
            DeleteCompletedItems = engine.MultiOp<Empty>();
            DeleteItem = engine.MultiOp<string>();
            EditItem = engine.MultiOp<string>();
            EditingItemId = engine.El<string>(null);
            FinishEditItem = engine.MultiOp<string>();
            ItemFactory = new TodoItem.Factory(cd, engine,
                ToggleItemComplete, EditingItemId, FinishEditItem);

            engine.Computer(cd,
                new object[] {
                    Items,
                    ToggleItemComplete.Applied,
                },
                () => Computers.UncompletedCount(
                    UncompletedCount,
                    Items
                )
            );

            engine.Computer(cd,
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
            );

            engine.Computer(cd,
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
            );
        }
    }

    public interface ITodoItem
    {
        string Id { get; }
        IEl<string> Content { get; }
        IEl<bool> IsCompleted { get; }
    }

    public interface ITodoItemFactory
    {
        ITodoItem Create(string id, string content);
        void Dispose(ITodoItem item);
    }

    public class TodoItem : ITodoItem
    {
        public readonly string id;
        public readonly IEl<string> content;
        public readonly IEl<bool> isCompleted;

        public TodoItem(
            CompositeDisposable cd,
            IEngine engine,
            IMultiOp<string> toggleComplete,
            IEl<string> editingItemId,
            IMultiOp<string> finishEdit,
            string id,
            string content)
        {
            this.id = id;
            this.content = engine.El(content);
            isCompleted = engine.El(false);

            cd = new CompositeDisposable();

            engine.Computer(cd,
                new object[] {
                    toggleComplete
                },
                () => Computers.TodoItem.IsCompleted(
                    isCompleted,
                    toggleComplete,
                    this.id
                )
            );

            engine.Computer(cd,
                new object[] {
                    editingItemId,
                    finishEdit
                },
                () => Computers.TodoItem.Content(
                    this.content,
                    editingItemId,
                    finishEdit,
                    this.id
                )
            );
        }

        public string Id { get { return id; } }
        public IEl<string> Content { get { return content; } }
        public IEl<bool> IsCompleted { get { return isCompleted; } }

        public class Factory : CompositeDisposableFactory<ITodoItem>, ITodoItemFactory
        {
            private readonly IEngine engine;
            private readonly IMultiOp<string> toggleComplete;
            private readonly IEl<string> editingItemId;
            private readonly IMultiOp<string> finishEdit;

            public Factory(
                CompositeDisposable cd,
                IEngine engine,
                IMultiOp<string> toggleComplete,
                IEl<string> editingItemId,
                IMultiOp<string> finishEdit
            )
            {
                this.engine = engine;
                this.toggleComplete = toggleComplete;
                this.editingItemId = editingItemId;
                this.finishEdit = finishEdit;

                cd.Add(this);
            }

            public ITodoItem Create(string id, string content)
            {
                var cd = new CompositeDisposable();
                var item = new TodoItem(cd, engine,
                    toggleComplete, editingItemId, finishEdit, id, content);
                Add(item, cd);
                return item;
            }

            public void Dispose(ITodoItem item)
            {
                Remove(item).Dispose();
            }
        }
    }
}