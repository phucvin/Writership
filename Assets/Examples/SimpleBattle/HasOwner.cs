namespace Examples.SimpleBattle
{
    public interface IHasOwner
    {
        IEntity Owner { get; }
    }

    public class HasOwner : IHasOwner
    {
        public IEntity Owner { get; private set; }

        public HasOwner(IEntity owner)
        {
            Owner = owner;
        }

        public void Setup()
        {
        }
    }
}