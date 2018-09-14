using NUnit.Framework;
using System;
using System.Collections.Generic;
using Writership;

public class Others
{
    private class Item
    {
        public readonly IEl<int> Value;

        public Item(IEngine engine, int value)
        {
            Value = engine.El(value);
        }
    }

    [Test]
    public void LiOpSimple()
    {
        var cd = new CompositeDisposable();
        var engine = new SinglethreadEngine();
        var li = engine.Li(new List<Item>());
        var liValueWatcher = engine.Wa(cd, li, item => item.Value);
        var op1 = engine.Op<Empty>();
        var op2 = engine.Op<Empty>();

        int total = 0;
        engine.Computer(cd,
            new object[] { li, liValueWatcher },
            () =>
            {
                var l = li.Read();
                total = 0;
                for (int i = 0, n = l.Count; i < n; ++i)
                {
                    total += l[i].Value.Read();
                }
            }
        );

        engine.Computer(cd,
            new object[] { op1, op2 },
            () =>
            {
                if (op1.Read().Count > 0)
                {
                    var l = li.AsWrite();
                    l.Add(new Item(engine, 3));
                    l.Add(new Item(engine, 2));
                }
                if (op2.Read().Count > 0)
                {
                    var l = li.Read();
                    for (int i = 0, n = l.Count; i < n; ++i)
                    {
                        l[i].Value.Write(l[i].Value.Read() + 1);
                    }
                }
            }
        );

        engine.Update();
        Assert.AreEqual(0, total);
        op1.Fire(Empty.Instance);
        engine.Update();
        Assert.AreEqual(5, total);
        op2.Fire(Empty.Instance);
        engine.Update();
        Assert.AreEqual(7, total);

        cd.Dispose();
        engine.Dispose();
    }

    [Test]
    public void Reduced()
    {
        var cd = new CompositeDisposable();
        var engine = new SinglethreadEngine();
        var tick = engine.Op<float>(reducer: (a, b) => a + b);
        var dummy = engine.El(10);

        float computerReduced = 0;
        float readerReduced = 0;
        engine.Computer(cd,
            new object[] { tick, dummy },
            () => computerReduced = tick.Reduced
        );
        engine.Reader(cd,
            new object[] { tick },
            () => readerReduced = tick.Reduced
        );

        tick.Fire(1f);
        tick.Fire(0.8f);
        engine.Update();
        Assert.AreEqual(1.8f, readerReduced, 0.0001f);
        Assert.AreEqual(1.8f, computerReduced, 0.0001f);

        tick.Fire(0.3f);
        engine.Update();
        Assert.AreEqual(0.3f, readerReduced, 0.0001f);

        dummy.Write(12);
        engine.Update();
        Assert.AreEqual(0f, computerReduced);
    }

    [Test]
    public void Writership()
    {
        var engine = new SinglethreadEngine();
        var dummy = engine.El(0);
        
        Action a1 = () => dummy.Write(3);
        Action a2 = () => dummy.Write(4);

        Assert.Throws<InvalidOperationException>(() =>
        {
            a1();
            engine.Update();
            a2();
        });
    }

    [Test]
    public void LiWriteProxy()
    {
        var cd = new CompositeDisposable();
        var engine = new SinglethreadEngine();
        var li = engine.Li(new List<int> { 1, 2, 3, 4 });

        int calledCount = 0;
        engine.Computer(cd, new object[] { li }, () =>
        {
            ++calledCount;
        });

        engine.Update();
        Assert.AreEqual(1, calledCount);

        {
            var proxy = li.AsWriteProxy();
            proxy.Commit();
        }
        engine.Update();
        Assert.AreEqual(1, calledCount);

        {
            var proxy = li.AsWriteProxy();
            proxy.RemoveAll(_ => false);
            proxy.Commit();
        }
        engine.Update();
        Assert.AreEqual(1, calledCount);

        {
            var proxy = li.AsWriteProxy();
            proxy.RemoveAt(0);
            proxy.Commit();
        }
        engine.Update();
        Assert.AreEqual(2, calledCount);
        Assert.AreEqual(new List<int> { 2, 3, 4 }, li.Read());
    }
}
