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

            public void Setup(IEngine engine, IWorld world)
            {
                this.engine = engine;
                this.world = world;
            }

            public IEntity Create(Info.Character info)
            {
                var e = new Entity();
                var cd = Add(e);
                // TODO Create
                var health = new Health(engine, info.Health);

                // TODO Setup and assign
                health.Setup(cd, engine, e, world);
                e.Health = health;

                return e;
            }

            public void Dispose(IEntity entity)
            {
                Remove(entity).Dispose();
            }
        }
    }
}