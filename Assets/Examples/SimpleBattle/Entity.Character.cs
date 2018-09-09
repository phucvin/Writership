using UnityEngine;
using Writership;

namespace Examples.SimpleBattle
{
    public interface ICharacterFactory
    {
        IEntity Create(Info.Character info);
        void Dispose(IEntity entity);
    }

    public partial class Entity
    {
        public class CharacterFactory : CompositeDisposableFactory<IEntity>, ICharacterFactory
        {
            private IEngine engine;
            private IWorld world;
            private Transform parent;

            public void Setup(IEngine engine, IWorld world, Transform parent)
            {
                this.engine = engine;
                this.world = world;
                this.parent = parent;
            }

            public IEntity Create(Info.Character info)
            {
                var e = new Entity();
                var cd = Add(e);

                var gameObject = Object.Instantiate((GameObject)info.Prototype, parent);
                cd.Add(new DestroyOnDispose(gameObject));
                
                // TODO Create
                var health = new Health(engine, info.Health);
                var spatial = gameObject.AddComponent<Spatial>().Ctor(engine);

                // TODO Setup and assign
                health.Setup(cd, engine, e, world);
                e.Health = health;
                //
                spatial.Setup();
                e.Spatial = spatial;

                return e;
            }

            public void Dispose(IEntity entity)
            {
                Remove(entity).Dispose();
            }
        }
    }
}