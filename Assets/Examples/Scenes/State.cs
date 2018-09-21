using Writership;

namespace Examples.Scenes
{
    public class State
    {
        public readonly El<int> Gold;
        public readonly Home Home;
        public readonly Inventory Inventory;

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            var state = this;

            Home.Setup(cd, engine, state);
        }

        public void SetupUnity(CompositeDisposable cd, IEngine engine)
        {
            var state = this;

            Home.SetupUnity(cd, engine, state);
        }
    }
}