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
        inc.Read().Returns(new List<Empty> { default(Empty) });
        dec.Read().Returns(new List<Empty>());

        Computers.Value(target, inc, dec);

        target.Received().Write(1);
    }
}
