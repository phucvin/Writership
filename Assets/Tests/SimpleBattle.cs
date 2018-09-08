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
        var me = new Entity();
        int armorValue = 10;
        var tick = new List<int> { 1, 1 };
        var healthCurrentModifier = Substitute.For<IModifierItem>();
        var modifiers = new List<IModifierItem>
        {
            healthCurrentModifier
        };
        var hitters = Substitute.For<ILi<IHitter>>();
        var damageHitter = Substitute.For<IDamageHitter>();
        var pureDamageHitter = Substitute.For<IPureDamageHitter>();
        var hitToArmorValue = Substitute.For<IEl<int>>();
        var hit = new List<World.Actions.Hit>
        {
            new World.Actions.Hit
            {
                FromOwner = new Entity(),
                FromHitters = hitters,

                To = me,
                ToArmorValue = hitToArmorValue,
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
        hitters.Read().Returns(new List<IHitter> { damageHitter, pureDamageHitter });
        damageHitter.Subtract.Read().Returns(17);
        pureDamageHitter.Subtract.Read().Returns(6);

        Health.ComputeCurrent(target, max, me, armorValue,
            tick, modifiers, hit, stickHits);

        target.Received().Write(87);
    }
}
