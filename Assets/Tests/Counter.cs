using Examples.Counter;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using Writership;

public class Counter
{
    [Test]
    public void Value()
    {
        var target = Substitute.For<IEl<int>>();
        var inc = Substitute.For<IOp<Empty>>();
        var dec = Substitute.For<IOp<Empty>>();

        target.Read().Returns(0);
        inc.Read().Returns(new List<Empty> { Empty.Instance });
        dec.Read().Returns(new List<Empty>());

        Computers.Value(target, inc, dec);

        target.Received().Write(1);
    }

    [Test]
    public void Simple()
    {
        var engine = new MultithreadEngine();
        var state = new State(engine);

        state.Increase.Fire(Empty.Instance);
        state.Increase.Fire(Empty.Instance);
        state.Decrease.Fire(Empty.Instance);
        engine.Update();

        engine.Update();
        engine.Dispose();

        Assert.AreEqual(1, state.Value.Read());
    }
}
