using UnityEngine;
using Writership;

namespace Examples.SimpleBattle
{
    public interface ISpatial
    {
        IEl<Vector3> Position { get; }
    }

    public class Spatial : MonoBehaviour, ISpatial
    {
        public IEl<Vector3> Position { get; private set; }

        public Spatial Ctor(IEngine engine)
        {
            Position = engine.El(transform.position);
            return this;
        }

        public void Setup()
        {
        }

        public void Update()
        {
            Position.Write(transform.position);
        }
    }
}