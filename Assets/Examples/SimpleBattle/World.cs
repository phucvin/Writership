using Writership;

namespace Examples.SimpleBattle
{
    public partial class World : Disposable
    {
        public readonly Actions actions; // TODO How to be uppercase
        public readonly ICharacterFactory CharacterFactory;

        public World(IEngine engine)
        {
            actions = new Actions(engine);
            CharacterFactory = new Entity.CharacterFactory();
        }

        public void Setup(IEngine engine)
        {
            actions.Setup(engine);
            ((Entity.CharacterFactory)CharacterFactory).Setup(engine,
                actions.tick, actions.hit);
#if DEBUG
            SetupGuards(engine);
#endif
        }
    }
}