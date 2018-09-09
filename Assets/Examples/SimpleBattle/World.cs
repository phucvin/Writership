using System;
using Writership;

namespace Examples.SimpleBattle
{
    public interface IWorld
    {
        IOps Ops { get; }
        ICharacterFactory CharacterFactory { get; }
        IModifierItemFactory ModifierItemFactory { get; }
        IStickHitItemFactory StickHitItemFactory { get; }
        IStickHitList StickHits { get; }
        IRandomSeed RandomSeed { get; }
    }

    public partial class World : IWorld
    {
        public IOps Ops { get; private set; }
        public ICharacterFactory CharacterFactory { get; private set; }
        public IModifierItemFactory ModifierItemFactory { get; private set; }
        public IStickHitItemFactory StickHitItemFactory { get; private set; }
        public IStickHitList StickHits { get; private set; }
        public IRandomSeed RandomSeed { get; private set; }

        public World(IEngine engine)
        {
            Ops = new Ops_(engine);
            CharacterFactory = new Entity.CharacterFactory();
            ModifierItemFactory = new ModifierItem.Factory();
            StickHitItemFactory = new StickHitItem.Factory();
            StickHits = new StickHitList(engine);
            RandomSeed = new RandomSeed(engine, new Random().Next());
        }

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            var world = this;

            ((Ops_)Ops).Setup();
            // TODO Get parent transform from outer scope
            ((Entity.CharacterFactory)CharacterFactory).Setup(engine, world, null);
            ((ModifierItem.Factory)ModifierItemFactory).Setup(engine, world);
            ((StickHitItem.Factory)StickHitItemFactory).Setup(engine, world);
            ((StickHitList)StickHits).Setup(cd, engine, world);
            ((RandomSeed)RandomSeed).Setup(cd, engine, world);

#if DEBUG
            SetupGuards(cd, engine);
#endif
        }
    }
}