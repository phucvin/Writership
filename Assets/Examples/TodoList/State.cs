﻿using System;
using System.Collections.Generic;
using Writership;

namespace Examples.TodoList
{
    public class State : IDisposable
    {
        public readonly IEl<int> NextId;
        public readonly IOp<string> CreateNewItem;
        public readonly ILi<ITodoItem> Items;
        public readonly IOp<string> ToggleItemComplete;
        public readonly IEl<int> UncompletedCount;
        public readonly IOp<Empty> DeleteCompletedItems;
        public readonly IOp<string> DeleteItem;
        public readonly IOp<string> EditItem;
        public readonly IEl<string> EditingItemId;
        public readonly IOp<string> FinishEditItem;
        public readonly ITodoItemFactory ItemFactory;

        private readonly CompositeDisposable cd;

        public State(IEngine engine)
        {
            NextId = engine.El(1);
            CreateNewItem = engine.Op<string>();
            Items = engine.Li(new List<ITodoItem>());
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

    public interface ITodoItem : IDisposable
    {
        string Id { get; }
        IEl<string> Content { get; }
        IEl<bool> IsCompleted { get; }
    }

    public interface ITodoItemFactory
    {
        ITodoItem Create(string id, string content);
    }

    public class TodoItem : ITodoItem
    {
        public readonly string id;
        public readonly IEl<string> content;
        public readonly IEl<bool> isCompleted;

        private readonly CompositeDisposable cd;

        public TodoItem(
            IEngine engine,
            IOp<string> toggleComplete,
            IEl<string> editingItemId,
            IOp<string> finishEdit,
            string id,
            string content)
        {
            this.id = id;
            this.content = engine.El(content);
            isCompleted = engine.El(false);

            cd = new CompositeDisposable();

            cd.Add(engine.RegisterComputer(
                new object[] {
                    toggleComplete
                },
                () => Computers.TodoItem.IsCompleted(
                    isCompleted,
                    toggleComplete,
                    this.id
                )
            ));

            cd.Add(engine.RegisterComputer(
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
            ));
        }

        public void Dispose()
        {
            cd.Dispose();
        }

        public string Id { get { return id; } }
        public IEl<string> Content { get { return content; } }
        public IEl<bool> IsCompleted { get { return isCompleted; } }

        public class Factory : ITodoItemFactory
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

            public ITodoItem Create(string id, string content)
            {
                return new TodoItem(engine, toggleComplete, editingItemId, finishEdit, id, content);
            }
        }
    }
}