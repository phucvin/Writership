using Writership;

namespace Examples.SimpleBattle
{
    public interface IHasOwner
    {
        IEntity Owner { get; }
    }

    public class HasOwner : Disposable, IHasOwner
    {
        public IEntity Owner { get; private set; }

        public HasOwner(IEngine engine, IEntity owner)
        {
            Owner = owner;
        }

        public void Setup(IEngine engine)
        {

        }
    }
}