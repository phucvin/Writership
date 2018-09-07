using Examples.TodoList;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using Writership;

public class TodoList
{
    [Test]
    public void UncompletedCount()
    {
        var target = Substitute.For<IEl<int>>();
        var items = Substitute.For <ILi<ITodoItem>>();
        var item1 = Substitute.For<ITodoItem>();
        var item2 = Substitute.For<ITodoItem>();

        item1.IsCompleted.Read().Returns(true);
        item2.IsCompleted.Read().Returns(false);
        items.Read().Returns(new List<ITodoItem>
        {
            item1, item2
        });

        Computers.UncompletedCount(target, items);

        target.Received().Write(1);
    }

    [Test]
    public void CreateNewItem()
    {
        var target = Substitute.For<ILi<ITodoItem>>();
        var targetAsWrite = new List<ITodoItem>();
        var nextId = Substitute.For<IEl<int>>();
        var newItem = Substitute.For<IOp<string>>();
        var deleteCompletedItems = Substitute.For<IOp<Empty>>();
        var deleteItem = Substitute.For<IOp<string>>();
        var itemFactory = Substitute.For<ITodoItemFactory>();
        var newItem1 = Substitute.For<ITodoItem>();
        var newItem2 = Substitute.For<ITodoItem>();

        target.AsWrite().Returns(targetAsWrite);
        nextId.Read().Returns(2);
        newItem.Read().Returns(new List<string>
        {
            "hello", "bye"
        });
        deleteCompletedItems.Read().Returns(new List<Empty>());
        deleteItem.Read().Returns(new List<string>());
        itemFactory.Create("2", "hello").Returns(newItem1);
        itemFactory.Create("3", "bye").Returns(newItem2);

        Computers.Items(target, nextId, newItem, deleteCompletedItems, deleteItem, itemFactory);
        
        Assert.AreEqual(new List<ITodoItem> { newItem1, newItem2 }, targetAsWrite);
    }

    [Test]
    public void CreateNewItemAndDelete()
    {
        var target = Substitute.For<ILi<ITodoItem>>();
        var targetAsWrite = new List<ITodoItem>();
        var nextId = Substitute.For<IEl<int>>();
        var newItem = Substitute.For<IOp<string>>();
        var deleteCompletedItems = Substitute.For<IOp<Empty>>();
        var deleteItem = Substitute.For<IOp<string>>();
        var itemFactory = Substitute.For<ITodoItemFactory>();
        var newItem1 = Substitute.For<ITodoItem>();
        var newItem2 = Substitute.For<ITodoItem>();

        target.AsWrite().Returns(targetAsWrite);
        nextId.Read().Returns(2);
        newItem.Read().Returns(new List<string>
        {
            "hello", "bye"
        });
        deleteCompletedItems.Read().Returns(new List<Empty>());
        deleteItem.Read().Returns(new List<string>
        {
            "2"
        });
        newItem1.Id.Returns("2");
        newItem2.Id.Returns("3");
        itemFactory.Create("2", "hello").Returns(newItem1);
        itemFactory.Create("3", "bye").Returns(newItem2);

        Computers.Items(target, nextId, newItem, deleteCompletedItems, deleteItem, itemFactory);

        Assert.AreEqual(new List<ITodoItem> { newItem2 }, targetAsWrite);
    }

    [Test]
    public void Simple()
    {
        var engine = new MultithreadEngine();
        var state = new State(engine);

        state.CreateNewItem.Fire("hello");
        engine.Update();
        state.EditItem.Fire("1");
        engine.Update();
        state.FinishEditItem.Fire("hello and bye");
        engine.Update();

        engine.Update();
        engine.Dispose();

        Assert.AreEqual("hello and bye", state.Items.Read()[0].Content.Read());
    }
}
