﻿using NUnit.Framework;
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
        var liValueWatcher = engine.Watcher().Setup(
            cd, engine,
            li, item => item.Value
        );
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
}
