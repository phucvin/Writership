using Writership;

namespace Examples.Scenes
{
    public class YesNoDialog
    {
        public readonly Scene Scene;

        public void Setup(CompositeDisposable cd, IEngine engine, State state)
        {
            Scene.Setup(cd, engine);
        }

        public void SetupUnity(CompositeDisposable cd, IEngine engine, State state)
        {
            Scene.SetupUnity(cd, engine);
        }
    }
}