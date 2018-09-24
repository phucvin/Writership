using Examples.SimpleBattle;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using Writership;

public class SimpleBattle
{
    [Test]
    public void HealthCurrent()
    {
        var target = Substitute.For<IEl<int>>();
        int max = 100;
        int regenSpeed = 2;
        int armorValue = 10;
        var tick = Substitute.For<IOp<Ops.Tick>>();
        var hitFrom = Substitute.For<IEntity>();
        var hitTo = Substitute.For<IEntity>();
        var hit = new List<Ops.Hit>
        {
            new Ops.Hit
            {
                From = hitFrom,
                To = hitTo,
                RandomSeed = 11
            }
        };
        var stickHits = new List<IStickHitItem>();
        var randomSeed = 198;

        target.Read().Returns(99);
        Ops.Tick t;
        tick.TryRead(out t).Returns(x =>
        {
            x[0] = new Ops.Tick { Dt = 2 };
            return true;
        });
        hitFrom.Hitters.Damage.Subtract.Read().Returns(17);
        hitFrom.Hitters.Damage.PureChance.Read().Returns(100);
        hitFrom.Hitters.Damage.CriticalChance.Read().Returns(50);

        Health.ComputeCurrent(target, max, regenSpeed,
            hitTo, armorValue, tick, hit, stickHits, randomSeed);

        target.Received().Write(83);
    }
}
