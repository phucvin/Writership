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

            public void Setup(CompositeDisposable cd, IEngine engine, IWorld world)
            {
                this.engine = engine;
                this.world = world;

                cd.Add(this);
            }

            public IEntity Create(Info.Character info)
            {
                var e = new Entity();
                var cd = Add(e);
                
                // TODO Create
                var health = new Health(engine, info.Health);
                var spatial = new Spatial(engine, info.SpatialFace);

                // TODO Setup and assign
                health.Setup(cd, engine, e, world);
                e.Health = health;
                //
                spatial.Setup(cd, engine);
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