using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Writership;

namespace Examples.Multiplayer
{
    public interface ISyncFireable
    {
        int Code { get; }
        void SyncFire(object obj);
    }

    public class SyncMultiOp<T> : ISyncFireable, IMultiOp<T>, IHaveCells
    {
        public int Code { get; private set; }
        private readonly IEngine engine;
        private readonly Networker networker;
        private readonly MultiOp<T> inner;

        public SyncMultiOp(IEngine engine, Networker networker, int code, bool allowWriters = false)
        {
            Code = code;
            this.engine = engine;
            this.networker = networker;
            inner = engine.MultiOp<T>(allowWriters);
        }

        public int Count { get { return Read().Count; } }
        public T First { get { return this[0]; } }
        public T Last { get { return this[Count - 1]; } }

        public T this[int i]
        {
            get { return Read()[i]; }
        }

        public void Fire(T value)
        {
            networker.Send(Code, value);
        }

        public void SyncFire(object obj)
        {
            var value = (T)obj;
            engine.MarkDirty(this, allowMultiple: true);
            inner.Fire(value);
        }

        public IList<T> Read()
        {
            return inner.Read();
        }

        public void ClearCell(int at)
        {
            // Ignore
        }

        public void CopyCell(int from, int to)
        {
            // Ignore
        }
    }

    public static class SyncOps
    {
        public struct TankPosition
        {
            public int Nid;
            public Vector3 Position;
        }
        public struct TankMovement
        {
            public int Nid;
            public Vector2 Movement;
        }
        public struct TankTeleport
        {
            public int Nid;
        }
    }

    public class SyncOps_
    {
        private readonly IEngine engine;
        private readonly Networker networker;

        public readonly SyncMultiOp<SyncOps.TankPosition> TankPosition;
        public readonly SyncMultiOp<SyncOps.TankMovement> TankMovement;
        public readonly SyncMultiOp<SyncOps.TankTeleport> TankTeleport;
        public readonly IList<ISyncFireable> All;

        private readonly List<ISyncFireable> all;

        public SyncOps_(IEngine engine, Networker networker)
        {
            this.engine = engine;
            this.networker = networker;

            all = new List<ISyncFireable>();
            All = all.AsReadOnly();

            TankPosition = Create<SyncOps.TankPosition>(1);
            TankMovement = Create<SyncOps.TankMovement>(2);
            TankTeleport = Create<SyncOps.TankTeleport>(3);
        }

        private SyncMultiOp<T> Create<T>(int code, bool allowWriters = false)
        {
            var op = new SyncMultiOp<T>(engine, networker, code, allowWriters);
            all.Add(op);
            return op;
        }
    }

    public class Networker
    {
        public readonly int Nid;
        public readonly El<int> ServerNid;
        public readonly SyncOps_ SyncOps;

        public Networker(IEngine engine, int nid, int serverNid)
        {
            Nid = nid;
            ServerNid = engine.El(serverNid);
            SyncOps = new SyncOps_(engine, this);
        }

        public bool IsServer { get { return Nid == ServerNid; } }
        public bool IsClient { get { return Nid != ServerNid; } }

        public bool IsMe(int nid) { return Nid == nid; }
        public bool IsPeer(int nid) { return Nid != nid; }

        public void Send(int code, object obj)
        {
            Debug.Log("Send " + code);
            // TODO
        }

        public void Receive(int code, object obj)
        {
            for (int i = 0, n = SyncOps.All.Count; i < n; ++i)
            {
                var it = SyncOps.All[i];
                if (it.Code == code)
                {
                    it.SyncFire(obj);
                    break;
                }
            }
        }
    }

    public class Tank
    {
        public readonly int Nid;
        public readonly ElWithRaw<Vector3, Vector3> Position;
        public readonly El<Quaternion> Rotation;
        public readonly El<Vector2> Movement;
        public readonly Op<Empty> Teleport;

        public Tank(IEngine engine, int nid)
        {
            Nid = nid;
            Position = engine.ElWithRaw(Vector3.zero);
            Rotation = engine.El(Quaternion.identity);
            Movement = engine.El(Vector2.zero);
            Teleport = engine.Op<Empty>();
        }

        public void Setup(CompositeDisposable cd, IEngine engine, Networker networker)
        {
            var syncPostion = networker.SyncOps.TankPosition;
            engine.Worker(cd, Dep.On(syncPostion, Position.Raw, Teleport), () =>
            {
                Vector3 newPosition = Position.Raw;
                if (networker.IsClient)
                {
                    for (int i = syncPostion.Count - 1; i >= 0; --i)
                    {
                        if (syncPostion[i].Nid == Nid)
                        {
                            newPosition = syncPostion[i].Position;
                            break;
                        }
                    }
                }
                if (Teleport)
                {
                    newPosition += Vector3.forward * 10;
                }
                Position.Write(newPosition);
            });

            var lastPosition = Position.Read();
            engine.Worker(cd, Dep.On(Position), () =>
            {
                if (!networker.IsServer) return;
                if (Position == lastPosition) return;

                networker.SyncOps.TankPosition.Fire(new SyncOps.TankPosition
                {
                    Nid = Nid,
                    Position = Position
                });
                lastPosition = Position;
            });

            var syncTeleport = networker.SyncOps.TankTeleport;
            engine.OpWorker(cd, Dep.On(Teleport), () =>
            {
                if (!networker.IsMe(Nid)) return;

                networker.SyncOps.TankTeleport.Fire(new SyncOps.TankTeleport
                {
                    Nid = Nid
                });
            });
            engine.Worker(cd, Dep.On(syncTeleport), () =>
            {
                if (!networker.IsServer) return;
                if (networker.IsMe(Nid)) return;

                for (int i = syncTeleport.Count - 1; i >= 0; --i)
                {
                    if (syncTeleport[i].Nid == Nid)
                    {
                        Teleport.Fire(Empty.Instance);
                        break;
                    }
                }
            });

            var syncMovement = networker.SyncOps.TankMovement;
            var lastMovement = Movement.Read();
            engine.Worker(cd, Dep.On(Movement), () =>
            {
                if (!networker.IsMe(Nid)) return;
                if (Movement == lastMovement) return;

                syncMovement.Fire(new SyncOps.TankMovement
                {
                    Nid = Nid,
                    Movement = Movement
                });
                lastMovement = Movement;
            });
            engine.Worker(cd, Dep.On(syncMovement), () =>
            {
                if (!networker.IsServer) return;
                if (networker.IsMe(Nid)) return;

                for (int i = syncMovement.Count - 1; i >= 0; --i)
                {
                    if (syncMovement[i].Nid == Nid)
                    {
                        Movement.Write(syncMovement[i].Movement);
                        break;
                    }
                }
            });
        }

        public void SetupTest(CompositeDisposable cd, IEngine engine, Networker networker)
        {
            Position.Raw.Write(Vector3.forward * 5);
            Teleport.Fire(Empty.Instance);
        }

        public void SetupUnity(CompositeDisposable cd, IEngine engine, Networker networker)
        {
            // TODO Create game object
            Transform transform = null;
            Rigidbody rb = null;

            engine.Mainer(cd, Dep.On(Position), () =>
            {
                // TODO Interpolate and/or extrapolate
                // but only if not server
                transform.position = Position;
            });

            if (networker.IsMe(Nid) || networker.IsServer)
            {
                Common.CoroutineExecutor.Instance.StartCoroutine(cd, FixedUpdate(rb));
                Common.CoroutineExecutor.Instance.StartCoroutine(cd, Update(transform));
            }
            if (networker.IsMe(Nid))
            {
                Common.CoroutineExecutor.Instance.StartCoroutine(cd, Input());
            }
        }

        private IEnumerator FixedUpdate(Rigidbody rb)
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
                rb.AddForce(Movement.Read());
            }
        }

        private IEnumerator Update(Transform transform)
        {
            while (true)
            {
                yield return null;
                Position.Raw.Write(transform.position);
            }
        }

        private IEnumerator Input()
        {
            while (true)
            {
                yield return null;
                Movement.Write(new Vector2(
                    UnityEngine.Input.GetAxis("Horizontal"),
                    UnityEngine.Input.GetAxis("Vertical")
                ));
            }
        }
    }
}