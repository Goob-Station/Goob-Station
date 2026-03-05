using System;
using System.Linq;
using Content.Server.Body.Systems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.Damageable;

[TestFixture]
[TestOf(typeof(DamageableSystem))]
// Goobstation - regression coverage for the complex-body damage routing optimizations.
public sealed class ComplexBodyDamageableTest
{
    private static readonly EntProtoId HumanPrototype = "MobHuman";
    private static readonly ProtoId<DamageTypePrototype> BluntDamageType = "Blunt";
    private static readonly ProtoId<DamageGroupPrototype> AirlossDamageGroup = "Airloss";

    private static FixedPoint2 SumBodyPartDamage(BodySystem bodySystem, EntityUid body)
    {
        var total = FixedPoint2.Zero;
        foreach (var part in bodySystem.GetBodyChildrenWithComponent<DamageableComponent>(body))
        {
            total += part.Component.TotalDamage;
        }

        return total;
    }

    private static FixedPoint2 SumDamage(IEntityManager entMan, EntityUid[] entities)
    {
        var total = FixedPoint2.Zero;
        foreach (var entity in entities)
        {
            total += entMan.GetComponent<DamageableComponent>(entity).TotalDamage;
        }

        return total;
    }

    [Test]
    public async Task ComplexBodiesKeepParentDamageInSync()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var entMan = server.EntMan;
        var protoMan = server.ProtoMan;
        var bodySystem = entMan.System<BodySystem>();
        var damageableSystem = entMan.System<DamageableSystem>();
        var map = await pair.CreateTestMap();

        EntityUid human = default;
        EntityUid rightArm = default;
        EntityUid rightLeg = default;
        DamageTypePrototype blunt = default!;

        await server.WaitPost(() =>
        {
            human = entMan.SpawnEntity(HumanPrototype, map.MapCoords);
            blunt = protoMan.Index(BluntDamageType);
            rightArm = bodySystem.GetBodyChildrenOfType(human, BodyPartType.Arm, symmetry: BodyPartSymmetry.Right).First().Id;
            rightLeg = bodySystem.GetBodyChildrenOfType(human, BodyPartType.Leg, symmetry: BodyPartSymmetry.Right).First().Id;

            damageableSystem.TryChangeDamage(human,
                new DamageSpecifier(blunt, FixedPoint2.New(10)),
                true,
                targetPart: TargetBodyPart.RightArm,
                canMiss: false);
            damageableSystem.TryChangeDamage(human,
                new DamageSpecifier(blunt, FixedPoint2.New(6)),
                true,
                targetPart: TargetBodyPart.RightLeg,
                canMiss: false);
            damageableSystem.TryChangeDamage(human,
                new DamageSpecifier(blunt, FixedPoint2.New(-4)),
                true,
                targetPart: TargetBodyPart.RightArm,
                canMiss: false);
        });

        await server.WaitRunTicks(1);

        await server.WaitAssertion(() =>
        {
            var humanDamageable = entMan.GetComponent<DamageableComponent>(human);
            var armDamageable = entMan.GetComponent<DamageableComponent>(rightArm);
            var legDamageable = entMan.GetComponent<DamageableComponent>(rightLeg);
            var bodyPartTotal = SumBodyPartDamage(bodySystem, human);

            Assert.Multiple(() =>
            {
                Assert.That(armDamageable.TotalDamage, Is.EqualTo(FixedPoint2.New(6)));
                Assert.That(legDamageable.TotalDamage, Is.EqualTo(FixedPoint2.New(6)));
                Assert.That(bodyPartTotal, Is.EqualTo(FixedPoint2.New(12)));
                Assert.That(humanDamageable.TotalDamage, Is.EqualTo(bodyPartTotal));
                Assert.That(humanDamageable.Damage.DamageDict["Blunt"], Is.EqualTo(bodyPartTotal));
            });
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task VitalOnlyDamageBypassesNonVitalTargeting()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var entMan = server.EntMan;
        var protoMan = server.ProtoMan;
        var bodySystem = entMan.System<BodySystem>();
        var damageableSystem = entMan.System<DamageableSystem>();
        var map = await pair.CreateTestMap();

        EntityUid human = default;
        EntityUid rightArm = default;
        EntityUid[] vitalParts = Array.Empty<EntityUid>();
        DamageGroupPrototype airloss = default!;
        FixedPoint2 initialArmDamage = FixedPoint2.Zero;

        await server.WaitPost(() =>
        {
            human = entMan.SpawnEntity(HumanPrototype, map.MapCoords);
            airloss = protoMan.Index(AirlossDamageGroup);
            rightArm = bodySystem.GetBodyChildrenOfType(human, BodyPartType.Arm, symmetry: BodyPartSymmetry.Right).First().Id;
            vitalParts = bodySystem.GetVitalBodyChildren(human).Select(part => part.Id).ToArray();
            initialArmDamage = entMan.GetComponent<DamageableComponent>(rightArm).TotalDamage;

            damageableSystem.TryChangeDamage(human,
                new DamageSpecifier(airloss, FixedPoint2.New(12)),
                true,
                targetPart: TargetBodyPart.RightArm,
                canMiss: false);
        });

        await server.WaitRunTicks(1);

        await server.WaitAssertion(() =>
        {
            var humanDamageable = entMan.GetComponent<DamageableComponent>(human);
            var armDamageable = entMan.GetComponent<DamageableComponent>(rightArm);
            var vitalDamage = SumDamage(entMan, vitalParts);

            Assert.Multiple(() =>
            {
                Assert.That(vitalParts, Has.Length.EqualTo(3));
                Assert.That(armDamageable.TotalDamage, Is.EqualTo(initialArmDamage));
                Assert.That(vitalDamage, Is.EqualTo(FixedPoint2.New(12)));
                Assert.That(humanDamageable.TotalDamage, Is.EqualTo(vitalDamage));
            });
        });

        await pair.CleanReturnAsync();
    }
}
