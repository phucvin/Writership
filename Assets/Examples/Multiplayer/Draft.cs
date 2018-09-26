using System.Collections;
using UnityEngine;
using Writership;

namespace Examples.Multiplayer
{
    public static class Sync
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

    public class Sync_
    {
        // TODO Custom multi op, fire to & read from separated buffers
        public readonly MultiOp<Sync.TankPosition> TankPosition;
        public readonly MultiOp<Sync.TankMovement> TankMovement;
        public readonly MultiOp<Sync.TankTeleport> TankTeleport;
    }

    public class Networker
    {
        public readonly int Nid;
        public readonly El<int> ServerNid;
        public readonly Sync_ Sync;

        public bool IsServer { get { return Nid == ServerNid; } }
        public bool IsClient { get { return Nid != ServerNid; } }

        public bool IsMe(int nid) { return Nid == nid; }
        public bool IsPeer(int nid) { return Nid != nid; }
    }

    public class Tank
    {
        public readonly int Nid;
        public readonly ElWithRaw<Vector3, Vector3> Position;
        public readonly El<Quaternion> Rotation;
        public readonly El<Vector2> Movement;
        public readonly Op<Empty> Teleport;

        public void Setup(CompositeDisposable cd, IEngine engine, Networker networker)
        {
            var syncPostion = networker.Sync.TankPosition;
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

            // TODO El.IsChanged can be useful here
            var lastPosition = Position.Read();
            engine.Worker(cd, Dep.On(Position), () =>
            {
                if (!networker.IsServer) return;
                if (Position == lastPosition) return;

                networker.Sync.TankPosition.Fire(new Sync.TankPosition
                {
                    Nid = Nid,
                    Position = Position
                });
                lastPosition = Position;
            });

            var syncTeleport = networker.Sync.TankTeleport;
            engine.Worker(cd, Dep.On(Teleport), () =>
            {
                if (!networker.IsMe(Nid)) return;

                networker.Sync.TankTeleport.Fire(new Sync.TankTeleport
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

            // TODO El.IsChanged can be useful here
            var syncMovement = networker.Sync.TankMovement;
            var lastMovement = Movement.Read();
            engine.Worker(cd, Dep.On(Movement), () =>
            {
                if (!networker.IsMe(Nid)) return;
                if (Movement == lastMovement) return;

                syncMovement.Fire(new Sync.TankMovement
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

        public void SetupUnity(CompositeDisposable cd, IEngine engine, Networker networker)
        {
            Transform transform = null;
            Rigidbody rb = null;

            engine.Mainer(cd, Dep.On(Position), () =>
            {
                // TODO Interpolate and/or extrapolate
                transform.position = Position;
            });

            if (networker.IsMe(Nid) || networker.IsServer)
            {
                Common.CoroutineExecutor.Instance.StartCoroutine(cd, FixedUpdate(rb));
                Common.CoroutineExecutor.Instance.StartCoroutine(cd, Update(transform));
            }
        }

        private IEnumerator FixedUpdate(Rigidbody rb)
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
                rb.velocity = Movement.Read();
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
    }
}