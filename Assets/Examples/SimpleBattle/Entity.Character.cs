using Writership;

namespace Examples.SimpleBattle
{
    public interface ICharacterFactory
    {
        IEntity Create(Info.Character info);
    }

    public partial class Entity
    {
        public class CharacterFactory : ICharacterFactory
        {
            private IEngine engine;
            private IOp<World.Actions.Tick> tick;
            private IOp<World.Actions.Hit> hit;

            public void Setup(IEngine engine,
                IOp<World.Actions.Tick> tick, IOp<World.Actions.Hit> hit)
            {
                this.engine = engine;
                this.tick = tick;
                this.hit = hit;
            }

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