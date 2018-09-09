using System;

namespace Examples.SimpleBattle
{
    public interface IHitter
    {
    }

    public abstract class Hitter : IHitter
    {
        public Hitter(Info.IHitter info)
        {
        }

        public static bool CanHit(IEntity from, IEntity to, Info.HitTo hitTo)
        {
            if (from.Team == null || to.Team == null) return false;

            switch (hitTo)
            {
                case Info.HitTo.Enemy:
                    return from.Team.Value.Read() != to.Team.Value.Read();

                case Info.HitTo.Teammate:
                    return from.Team.Value.Read() == to.Team.Value.Read();

                case Info.HitTo.SelfAndTeammate:
                    return from == to || from.Team.Value.Read() == to.Team.Value.Read();

                default:
                    throw new NotImplementedException();
            }
        }
    }
}