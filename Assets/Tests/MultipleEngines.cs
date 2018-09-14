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

                engine.Computer(cd,
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
                );

                engine.Computer(cd,
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
                );
            }

            public void Dispose()
            {
                cd.Dispose();
            }
        }
    }

    public class Visuals : IDisposable
    {
        private readonly IEngine engine;
        private readonly State state;

        private readonly CompositeDisposable cd = new CompositeDisposable();

        public Visuals()
        {
            cd.Add(engine = new MultithreadEngine());
            cd.Add(state = new State(engine));
        }

        public void Dispose()
        {
            cd.Dispose();
        }

        public void AddSprite(Sprite s)
        {
            state.Add.Fire(s);
            engine.Update();
        }

        public void Update(int dt)
        {
            state.Tick.Fire(dt);
            engine.Update();
        }

        public Sprite GetSprite(string id)
        {
            engine.Update();
            var ids = state.Ids.Read();
            var sheets = state.Sheets.Read();
            var currentFrameIndexes = state.CurrentFrameIndexes.Read();
            var totalFrames = state.TotalFrames.Read();
            var speeds = state.Speeds.Read();

            for (int i = 0, n = ids.Count; i < n; ++i)
            {
                if (ids[i] == id)
                {
                    return new Sprite
                    {
                        Id = ids[i],
                        Sheet = sheets[i],
                        CurrentFrameIndex = currentFrameIndexes[i],
                        TotalFrame = totalFrames[i],
                        Speed = speeds[i]
                    };
                }
            }

            return default(Sprite);
        }

        public void GetSpriteComponentArrays(out IList<string> ids, out IList<object> sheets, out IList<int> currentFrameIndexes)
        {
            engine.Update();
            ids = state.Ids.Read();
            sheets = state.Sheets.Read();
            currentFrameIndexes = state.CurrentFrameIndexes.Read();
        }

        public struct Sprite
        {
            public string Id;
            public object Sheet;
            public int CurrentFrameIndex;
            public int TotalFrame;
            public int Speed;
        }

        public class State : IDisposable
        {
            public readonly ILi<string> Ids;
            public readonly ILi<object> Sheets;
            public readonly ILi<int> CurrentFrameIndexes;
            public readonly ILi<int> TotalFrames;
            public readonly ILi<int> Speeds;
            public readonly IOp<int> Tick;
            public readonly IOp<Sprite> Add;

            private readonly CompositeDisposable cd = new CompositeDisposable();

            public State(IEngine engine)
            {
                Ids = engine.Li(new List<string>());
                Sheets = engine.Li(new List<object>());
                CurrentFrameIndexes = engine.Li(new List<int>());
                TotalFrames = engine.Li(new List<int>());
                Speeds = engine.Li(new List<int>());
                Tick = engine.Op<int>();
                Add = engine.Op<Sprite>();

                engine.Computer(cd,
                    new object[] { Tick, Add },
                    () =>
                    {
                        var tick = Tick.Read();
                        var add = Add.Read();

                        if (tick.Count <= 0 && add.Count <= 0) return;

                        var currentFrameIndexes = CurrentFrameIndexes.AsWrite();

                        {
                            var totalFrames = TotalFrames.Read();
                            var speeds = Speeds.Read();

                            for (int t = 0, c = tick.Count; t < c; ++t)
                            {
                                var dt = tick[t];

                                for (int i = 0, n = Ids.Read().Count; i < n; ++i)
                                {
                                    var current = currentFrameIndexes[i];
                                    current = (current + dt * speeds[i]) % totalFrames[i];
                                    currentFrameIndexes[i] = current;
                                }
                            }
                        }

                        if (add.Count > 0)
                        {
                            for (int i = 0, n = add.Count; i < n; ++i)
                            {
                                var s = add[i];
                                currentFrameIndexes.Add(s.CurrentFrameIndex);
                            }
                        }
                    }
                );

                engine.Computer(cd,
                    new object[] { Add },
                    () =>
                    {
                        var add = Add.Read();

                        if (add.Count <= 0) return;

                        var ids = Ids.AsWrite();
                        var sheets = Sheets.AsWrite();
                        var totalFrames = TotalFrames.AsWrite();
                        var speeds = Speeds.AsWrite();

                        for (int i = 0, n = add.Count; i < n; ++i)
                        {
                            var s = add[i];
                            ids.Add(s.Id);
                            sheets.Add(s.Sheet);
                            totalFrames.Add(s.TotalFrame);
                            speeds.Add(s.Speed);
                        }
                    }
                );
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
