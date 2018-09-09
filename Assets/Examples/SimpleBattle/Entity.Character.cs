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
            private IOp<Ops.Tick> tick;
            private IOp<Ops.Hit> hit;

            public void Setup(IEngine engine,
                IOp<Ops.Tick> tick, IOp<Ops.Hit> hit)
            {
                this.engine = engine;
                this.tick = tick;
                this.hit = hit;
            }

            public void Dispose() { }

            public IEntity Create(Info.Character info)
            {
                var e = new Entity();
                // TODO Create
                var health = new Health(engine, info.Health);

                // TODO Setup and assign
                health.Setup(engine, e, null, tick, null, hit, null, null);
                e.Health = health;

                return e;
            }
        }
    }
}