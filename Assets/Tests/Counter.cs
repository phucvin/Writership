using Examples.Counter;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using Writership;

public class Counter
{
    [Test]
    public void Simple()
    {
        var value = Substitute.For<IEl<int>>();
        var inc = Substitute.For<IOp<Empty>>();
        var dec = Substitute.For<IOp<Empty>>();

        value.Read().Returns(0);
        inc.Read().Returns(new List<Empty> { default(Empty) });
        dec.Read().Returns(new List<Empty>());

        Computers.Value(value, inc, dec);

        value.Received().Write(1);
    }
}
