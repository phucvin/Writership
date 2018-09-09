using Examples.SimpleBattle;
using Info = Examples.SimpleBattle.Info;
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
        int armorValue = 10;
        var tick = new List<Ops.Tick>
        {
            new Ops.Tick { Dt = 1 },
            new Ops.Tick { Dt = 1 }
        };
        var healthCurrentModifier = Substitute.For<IModifierItem>();
        var modifiers = new List<IModifierItem>
        {
            healthCurrentModifier
        };
        var hitFrom = Substitute.For<IEntity>();
        var hitTo = Substitute.For<IEntity>();
        var hit = new List<Ops.Hit>
        {
            new Ops.Hit
            {
                From = hitFrom,
                To = hitTo
            }
        };
        var stickHits = new List<IStickHitItem>();
        var randomSeed = Substitute.For<IRandomSeed>();

        target.Read().Returns(99);
        healthCurrentModifier.Info.Returns(new Info.HealthCurrentModifier
        {
            Add = 2,
            Duration = 100
        });
        healthCurrentModifier.Remain.Read().Returns(100);
        hitFrom.Hitters.Damage.Subtract.Read().Returns(17);
        hitFrom.Hitters.Damage.PureChance.Read().Returns(100);
        hitFrom.Hitters.Damage.CriticalChance.Read().Returns(50);
        randomSeed.Value.Read().Returns(198);

        Health.ComputeCurrent(target, max, hitTo, armorValue,
            tick, modifiers, hit, stickHits, randomSeed);

        target.Received().Write(83);
    }
}
