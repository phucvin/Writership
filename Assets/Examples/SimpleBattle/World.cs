using System;
using Writership;

namespace Examples.SimpleBattle
{
    public interface IWorld
    {
        IOps Ops { get; }
        ICharacterFactory CharacterFactory { get; }
        IModifierItemFactory ModifierItemFactory { get; }
        IStickHitList StickHits { get; }
        IRandomSeed RandomSeed { get; }
    }

    public partial class World : Disposable, IWorld
    {
        public IOps Ops { get; private set; }
        public ICharacterFactory CharacterFactory { get; private set; }
        public IModifierItemFactory ModifierItemFactory { get; private set; }
        public IStickHitList StickHits { get; private set; }
        public IRandomSeed RandomSeed { get; private set; }

        public World(IEngine engine)
        {
            Ops = cd.Add(new Ops_(engine));
            CharacterFactory = cd.Add(new Entity.CharacterFactory());
            ModifierItemFactory = cd.Add(new ModifierItem.Factory());
            StickHits = cd.Add(new StickHitList(engine));
            RandomSeed = cd.Add(new RandomSeed(engine, new Random().Next()));
        }

        public void Setup(IEngine engine)
        {
            var world = this;

            ((Ops_)Ops).Setup(engine);
            ((Entity.CharacterFactory)CharacterFactory).Setup(engine, world);
            ((ModifierItem.Factory)ModifierItemFactory).Setup(engine, world);
            ((StickHitList)StickHits).Setup(engine);
            ((RandomSeed)RandomSeed).Setup(engine, world);

#if DEBUG
            SetupGuards(engine);
#endif
        }
    }
}