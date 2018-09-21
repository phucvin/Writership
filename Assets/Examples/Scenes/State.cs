using Writership;

namespace Examples.Scenes
{
    public class State
    {
        public readonly El<int> Gold;
        public readonly SceneStack SceneStack;
        public readonly Home Home;
        public readonly Inventory Inventory;

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            var state = this;

            SceneStack.Setup(cd, engine);
            Home.Setup(cd, engine, state);
        }

        public void SetupUnity(CompositeDisposable cd, IEngine engine)
        {
            var state = this;

            SceneStack.SetupUnity(cd, engine);
            Home.SetupUnity(cd, engine, state);
        }
    }
}