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
        var tick = new List<World.Actions.Tick>
        {
            new World.Actions.Tick { Dt = 1 },
            new World.Actions.Tick { Dt = 1 }
        };
        var healthCurrentModifier = Substitute.For<IModifierItem>();
        var modifiers = new List<IModifierItem>
        {
            healthCurrentModifier
        };
        var hitFrom = Substitute.For<IEntity>();
        var hitTo = Substitute.For<IEntity>();
        var hit = new List<World.Actions.Hit>
        {
            new World.Actions.Hit
            {
                From = hitFrom,
                To = hitTo,
                RandomSeeds = new RandomSeeds
                {
                    DamagePureChance = 1,
                    DamageCriticalChance = 2,
                }
            }
        };
        var stickHits = new List<IStickHitItem>();

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

        Health.ComputeCurrent(target, max, hitTo, armorValue,
            tick, modifiers, hit, stickHits);

        target.Received().Write(83);
    }
}
