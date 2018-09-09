using Writership;

namespace Examples.SimpleBattle
{
    public partial class World : Disposable
    {
        public readonly Actions actions;

        public World(IEngine engine)
        {
            actions = new Actions(engine);
        }

        public void Setup(IEngine engine)
        {
#if DEBUG
            SetupGuards(engine);
#endif
        }
    }
}