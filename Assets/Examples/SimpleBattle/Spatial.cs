using UnityEngine;
using Writership;

namespace Examples.SimpleBattle
{
    public interface ISpatial
    {
        IEl<Vector3?> Position { get; }
        IEl<int> Face { get; }
    }

    public class Spatial : ISpatial
    {
        public IEl<Vector3?> Position { get; private set; }
        public IEl<int> Face { get; private set; }

        public Spatial(IEngine engine, int face)
        {
            Position = engine.El<Vector3?>(null);
            Face = engine.El(face);
        }

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            // Position is external, no need compute here

            // TODO Compute Face
        }
    }
}