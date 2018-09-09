using System;
using Writership;

namespace Examples.SimpleBattle
{
    public interface ICharacterFactory
    {
        IEntity Create(Info.Character info);
    }

    public partial class Entity
    {
        public class CharacterFactory : ICharacterFactory, IDisposable
        {
            private IEngine engine;
            private IWorld world;

            public void Setup(IEngine engine, IWorld world)
            {
                this.engine = engine;
                this.world = world;
            }

            public void Dispose() { }

            public IEntity Create(Info.Character info)
            {
                var e = new Entity();
                // TODO Create
                var health = new Health(engine, info.Health);

                // TODO Setup and assign
                health.Setup(engine, e, world);
                e.Health = health;

                return e;
            }
        }
    }
}