using Writership;

namespace Examples.SimpleBattle
{
    public interface ITeam
    {
        IEl<int> Value { get; }
    }

    public class Team : Disposable, ITeam
    {
        public IEl<int> Value { get; private set; }

        public Team(IEngine engine, Info.Team info)
        {
            Value = engine.El(info.Value);
        }

        public void Setup(IEngine engine)
        {
        }
    }
}