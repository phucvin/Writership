using NUnit.Framework;
using System;
using System.Collections.Generic;
using Writership;

public class MultipleEngines
{
    public class Physics : IDisposable
    {
        private readonly IEngine engine;
        private readonly State state;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public Physics()
        {
            cd.Add(engine = new MultithreadEngine());
            cd.Add(state = new State(engine));
        }

        public void Dispose()
        {
            cd.Dispose();
        }

        public void AddRigidbody(Rigidbody r)
        {
            state.Add.Fire(r);
            engine.Update();
        }

        public void Update(int dt)
        {
            state.Tick.Fire(dt);
            engine.Update();
        }

        public Rigidbody GetRigidbody(string id)
        {
            engine.Update();
            var ids = state.Ids.Read();
            var xs = state.Xs.Read();
            var ys = state.Ys.Read();
            var vxs = state.Vxs.Read();
            var vys = state.Vys.Read();

            for (int i = 0, n = ids.Count; i < n; ++i)
            {
                if (ids[i] == id)
                {
                    return new Rigidbody
                    {
                        Id = ids[i],
                        X = xs[i],
                        Y = ys[i],
                        Vx = vxs[i],
                        Vy = vys[i]
                    };
                }
            }

            return default(Rigidbody);
        }

        public void GetRigidbodyComponentArrays(out IList<string> ids, out IList<int> xs, out IList<int> ys)
        {
            engine.Update();
            ids = state.Ids.Read();
            xs = state.Xs.Read();
            ys = state.Ys.Read();
        }

        public struct Rigidbody
        {
            public string Id;
            public int X;
            public int Y;
            public int Vx;
            public int Vy;
        }

        public class State : IDisposable
        {
            public readonly ILi<string> Ids;
            public readonly ILi<int> Xs;
            public readonly ILi<int> Ys;
            public readonly ILi<int> Vxs;
            public readonly ILi<int> Vys;
            public readonly IOp<int> Tick;
            public readonly IOp<Rigidbody> Add;

            private readonly CompositeDisposable cd = new CompositeDisposable();

            public State(IEngine engine)
            {
                Ids = engine.Li(new List<string>());
                Xs = engine.Li(new List<int>());
                Ys = engine.Li(new List<int>());
                Vxs = engine.Li(new List<int>());
                Vys = engine.Li(new List<int>());
                Tick = engine.Op<int>();
                Add = engine.Op<Rigidbody>();

                cd.Add(engine.RegisterComputer(
                    new object[] { Tick, Add },
                    () =>
                    {
                        var tick = Tick.Read();
                        var add = Add.Read();

                        if (tick.Count <= 0 && add.Count <= 0) return;

                        var xs = Xs.AsWrite();
                        var ys = Ys.AsWrite();

                        {
                            var vxs = Vxs.Read();
                            var vys = Vys.Read();

                            for (int t = 0, c = tick.Count; t < c; ++t)
                            {
                                var dt = tick[t];

                                for (int i = 0, n = Ids.Read().Count; i < n; ++i)
                                {
                                    xs[i] += vxs[i] * dt;
                                    ys[i] += vys[i] * dt;
                                }
                            }
                        }

                        if (add.Count > 0)
                        {
                            for (int i = 0, n = add.Count; i < n; ++i)
                            {
                                var r = add[i];
                                xs.Add(r.X);
                                ys.Add(r.Y);
                            }
                        }
                    }
                ));

                cd.Add(engine.RegisterComputer(
                    new object[] { Add },
                    () =>
                    {
                        var add = Add.Read();

                        if (add.Count <= 0) return;

                        var ids = Ids.AsWrite();
                        var vxs = Vxs.AsWrite();
                        var vys = Vys.AsWrite();

                        for (int i = 0, n = add.Count; i < n; ++i)
                        {
                            var r = add[i];
                            ids.Add(r.Id);
                            vxs.Add(r.Vx);
                            vys.Add(r.Vy);
                        }
                    }
                ));
            }

            public void Dispose()
            {
                cd.Dispose();
            }
        }
    }

    [Test]
    public void PhysicsOnly()
    {
        var physics = new Physics();

        physics.AddRigidbody(new Physics.Rigidbody
        {
            Id = "1",
            X = 0,
            Y = 1,
            Vx = 2,
            Vy = 0
        });
        Assert.AreEqual(0, physics.GetRigidbody("1").X);

        physics.Update(2);
        Assert.AreEqual(
            new Physics.Rigidbody
            {
                Id = "1",
                X = 4,
                Y = 1,
                Vx = 2,
                Vy = 0
            },
            physics.GetRigidbody("1")
        );

        physics.Dispose();
    }
}
