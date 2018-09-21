using Writership;

namespace Examples.Scenes
{
    public class SceneStack
    {
        public readonly Li<Scene> Scenes;
        public readonly El<bool> IsBusy;
        public readonly Op<Empty> Back;

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
        }

        public void SetupUnity(CompositeDisposable cd, IEngine engine)
        {
        }
    }
}