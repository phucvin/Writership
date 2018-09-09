using Writership;

namespace Examples.SimpleBattle
{
    public partial class World : Disposable
    {
        public readonly IOps Ops;
        public readonly ICharacterFactory CharacterFactory;

        public World(IEngine engine)
        {
            Ops = cd.Add(new Ops_(engine));
            CharacterFactory = cd.Add(new Entity.CharacterFactory());
        }

        public void Setup(IEngine engine)
        {
            ((Ops_)Ops).Setup(engine);
            ((Entity.CharacterFactory)CharacterFactory).Setup(engine,
                Ops.Tick, Ops.Hit);

#if DEBUG
            SetupGuards(engine);
#endif
        }
    }
}