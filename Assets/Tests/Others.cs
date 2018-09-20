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

    private struct HttpError
    {
        public bool DidSent;
        public int StatusCode;
        public bool IsSuccess;
    }

    private class HttpOp<TReq, TRes>
    {
        private readonly bool isSingle;
        private readonly string url;

        public readonly Op<TReq> Request;
        public readonly Op<TRes> Response;
        public readonly Op<HttpError> Error;

        private readonly El<int> requesting;

        public HttpOp(IEngine engine, string url, bool isSingle = false, bool allowWriters = false)
        {
            this.isSingle = isSingle;
            this.url = url;

            Request = engine.Op<TReq>(allowWriters);
            Response = engine.Op<TRes>();
            Error = engine.Op<HttpError>();
            requesting = engine.El(0);
        }

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            engine.Computer(cd, Dep.On(Request, Response, Error), () =>
            {
                if (requesting > 0 && isSingle) return;
                int result = requesting + (isSingle ? Math.Min(1, Request.Count) : Request.Count) - Response.Count - Error.Count;
                if (result < 0) throw new InvalidOperationException();
                requesting.Write(result);
            });

            engine.Reader(cd, Dep.On(Request), () =>
            {
                // TODO Fix unsync requesting
                if (requesting > 0 && isSingle)
                {
                    // TODO Warning or fire another op to tell missed http request
                    return;
                }

                for (int i = 0, n = isSingle ? Math.Min(1, Request.Count) : Request.Count; i < n; ++i)
                {
                    Error.Fire(new HttpError { DidSent = false });
                }
            });
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

    [Test]
    public void IsChanged()
    {
        var cd = new CompositeDisposable();
        var engine = new SinglethreadEngine();
        var i1 = engine.El(1);
        var i2 = engine.El(2);

        bool i1Changed = false;
        bool i2Changed = false;
        engine.Computer(cd, new object[] { i1, i2 }, () =>
        {
#pragma warning disable CS0612 // Type or member is obsolete
            i1Changed = i1.IsChanged;
            i2Changed = i2.IsChanged;
#pragma warning restore CS0612 // Type or member is obsolete
        });

        Assert.AreEqual(false, i1Changed);
        Assert.AreEqual(false, i2Changed);

        i1.Write(11);
        engine.Update();
        Assert.AreEqual(true, i1Changed);
        Assert.AreEqual(false, i2Changed);

        i2.Write(22);
        engine.Update();
        Assert.AreEqual(false, i1Changed);
        Assert.AreEqual(true, i2Changed);

        i1.Write(111);
        i2.Write(222);
        engine.Update();
        Assert.AreEqual(true, i1Changed);
        Assert.AreEqual(true, i2Changed);
    }
}
