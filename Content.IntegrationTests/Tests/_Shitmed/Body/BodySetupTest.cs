// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using System.Linq;
using Content.Server.Administration.Systems;
using Content.Server.Body.Systems;
using Content.Server.Hands.Systems;
using Content.Server.Tools.Innate;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Components;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.IntegrationTests.Tests._Shitmed.Body;

[TestFixture]
public sealed class BodySetupTest
{
    /// <summary>
    /// A list of species that are ignored by all the tests here.
    /// </summary>
    private readonly HashSet<string> _ignoredPrototypes = new()
    {
        "Skeleton",
        "Cyborg" // Since cyborgs are now a species just for appearance comps, we have to add em here.
    };

    /// <summary>
    /// A list of species that are ignored by the consciousness test.
    /// </summary>
    private readonly HashSet<string> _ignoredConsciousnessPrototypes = new()
    {
        "Cyborg",
        "IPC",
        "Skeleton",
    };

    // This test is kinda useless for us since the only place where we use InnateToolComponent is fuckin behonkers lmao.
    /*[Test]
    public async Task InnateToolTest()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true,
            Connected = true,
            InLobby = false,
        });

        var server = pair.Server;

        var prototypeManager = server.ResolveDependency<IPrototypeManager>();
        var handsSys = server.EntMan.System<HandsSystem>();
        var compFactory = server.ResolveDependency<IComponentFactory>();

        var testMap = await pair.CreateTestMap();

        //var ticker = server.System<GameTicker>();
        //Assert.That(ticker.RunLevel, Is.EqualTo(GameRunLevel.InRound));

        foreach (var proto in prototypeManager.EnumeratePrototypes<EntityPrototype>())
        {
            var skip = false;
            InnateToolComponent toolComponent = null;
            await server.WaitAssertion(() =>
            {
                if (!proto.TryGetComponent(out toolComponent, compFactory))
                    skip = true;
            });

            if(skip)
                continue;

            var dummy = EntityUid.Invalid;
            await server.WaitAssertion(() =>
            {
                dummy = server.EntMan.Spawn(proto.ID, testMap.MapCoords);
            });
            await server.WaitIdleAsync();
            await server.WaitRunTicks(2);
            await server.WaitAssertion(() =>
            {
                Assert.That(dummy, Is.Not.EqualTo(EntityUid.Invalid));
                var handCount = handsSys.EnumerateHands(dummy).Count();
                Assert.That(handCount, Is.GreaterThanOrEqualTo(toolComponent.Tools.Count), $"hands {proto.ID}");
                server.EntMan.DeleteEntity(dummy);
            });
        }


        await pair.CleanReturnAsync();
    }*/

    [Test]
    public async Task AllSpeciesHaveLegs()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true,
            Connected = true,
            InLobby = false,
        });

        var server = pair.Server;
        var bodySys = server.EntMan.System<BodySystem>();

        foreach (var speciesPrototype in server.ProtoMan.EnumeratePrototypes<SpeciesPrototype>())
        {
            if (_ignoredPrototypes.Contains(speciesPrototype.ID))
                continue;

            var dummy = EntityUid.Invalid;
            await server.WaitAssertion(() =>
            {
                dummy = server.EntMan.Spawn(speciesPrototype.Prototype);
            });
            await server.WaitIdleAsync();
            await server.WaitRunTicks(2);
            await server.WaitAssertion(() =>
            {
                Assert.That(dummy, Is.Not.EqualTo(EntityUid.Invalid));
                var bodyComp = server.EntMan.GetComponent<BodyComponent>(dummy);
                var legs = bodyComp.LegEntities;
                var legsCount = bodySys.GetBodyPartCount(dummy, BodyPartType.Leg);
                Assert.That(legsCount, Is.EqualTo(legs.Count));
                Assert.That(legsCount, Is.GreaterThanOrEqualTo(2), $"legs {speciesPrototype.ID}({speciesPrototype.Prototype})");
            });

        }

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task AllSpeciesHaveHands()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true,
            Connected = true,
            InLobby = false,
        });

        var server = pair.Server;
        var handsSys = server.EntMan.System<HandsSystem>();

        foreach (var speciesPrototype in server.ProtoMan.EnumeratePrototypes<SpeciesPrototype>())
        {
            if (_ignoredPrototypes.Contains(speciesPrototype.ID))
                continue;

            var dummy = EntityUid.Invalid;
            await server.WaitAssertion(() =>
            {
                dummy = server.EntMan.Spawn(speciesPrototype.Prototype);
            });
            await server.WaitIdleAsync();
            await server.WaitRunTicks(2);
            await server.WaitAssertion(() =>
            {
                Assert.That(dummy, Is.Not.EqualTo(EntityUid.Invalid));
                var handCount = handsSys.EnumerateHands(dummy).Count();
                Assert.That(handCount, Is.GreaterThanOrEqualTo(2), $"hands {speciesPrototype.ID}({speciesPrototype.Prototype})");
            });

        }

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task AllSpeciesAreConscious()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true,
            Connected = true,
            InLobby = false,
        });

        var server = pair.Server;

        var entMan = server.ResolveDependency<IEntityManager>();
        var consciousnessSystem = entMan.System<ConsciousnessSystem>();

        await server.WaitAssertion(() =>
        {
            foreach (var speciesPrototype in server.ProtoMan.EnumeratePrototypes<SpeciesPrototype>())
            {
                if (_ignoredConsciousnessPrototypes.Contains(speciesPrototype.ID))
                    continue;

                var dummy = entMan.Spawn(speciesPrototype.Prototype);

                Assert.Multiple(() =>
                {
                    Assert.That(dummy, Is.Not.EqualTo(EntityUid.Invalid), $"Failed species to pass the test: {speciesPrototype.ID}");
                    Assert.That(entMan.TryGetComponent(dummy, out ConsciousnessComponent consciousness), $"Failed species to pass the test: {speciesPrototype.ID}");

                    Assert.That(consciousnessSystem.TryGetNerveSystem(dummy, out var dummyNerveSys));

                    Assert.That(entMan.HasComponent<OrganComponent>(dummyNerveSys), $"Failed species to pass the test: {speciesPrototype.ID}, organ {dummyNerveSys}");
                    Assert.That(entMan.HasComponent<ConsciousnessRequiredComponent>(dummyNerveSys), $"Failed species to pass the test: {speciesPrototype.ID}");

                    Assert.That(consciousnessSystem.CheckConscious(dummy, consciousness), $"Failed species to pass the test: {speciesPrototype.ID}");
                });
            }
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task AllSpeciesCanBeRejuvenated()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true,
            Connected = true,
            InLobby = false,
        });

        var server = pair.Server;

        var entMan = server.ResolveDependency<IEntityManager>();
        var bodySystem = entMan.System<BodySystem>();
        var woundSystem = entMan.System<WoundSystem>();
        var consciousnessSystem = entMan.System<ConsciousnessSystem>();
        var rejuvenateSystem = entMan.System<RejuvenateSystem>();

        await server.WaitAssertion(() =>
        {
            foreach (var speciesPrototype in server.ProtoMan.EnumeratePrototypes<SpeciesPrototype>())
            {
                if (_ignoredPrototypes.Contains(speciesPrototype.ID))
                    continue;

                var dummy = entMan.Spawn(speciesPrototype.Prototype);

                var initialBodyPartCount = bodySystem.GetBodyPartCount(dummy, BodyPartType.Head);
                var headEntity = bodySystem.GetBodyChildrenOfType(dummy, BodyPartType.Head).FirstOrDefault();
                var groinEntity = bodySystem.GetBodyChildrenOfType(dummy, BodyPartType.Groin).FirstOrDefault();

                Assert.Multiple(() =>
                {
                    Assert.That(bodySystem.TryGetParentBodyPart(headEntity.Id, out var parentPart, out _), $"Failed species to pass the test: {speciesPrototype.ID}");
                    Assert.That(parentPart, Is.Not.Null, $"Failed species to pass the test: {speciesPrototype.ID}");

                    Assert.That(entMan.TryGetComponent(headEntity.Id, out WoundableComponent woundable), $"Failed species to pass the test: {speciesPrototype.ID}");
                    Assert.That(entMan.TryGetComponent(groinEntity.Id, out WoundableComponent groinWoundable), $"Failed species to pass the test: {speciesPrototype.ID}");

                    // Destroy the head, and damage the groin so we can check.
                    woundSystem.DestroyWoundable(parentPart.Value, headEntity.Id, woundable);
                    woundSystem.TryInduceWound(groinEntity.Id, "Blunt", 25f, out _, groinWoundable);

                    rejuvenateSystem.PerformRejuvenate(dummy);

                    Assert.That(initialBodyPartCount, Is.EqualTo(bodySystem.GetBodyPartCount(dummy, BodyPartType.Head)), $"Failed species to pass the test: {speciesPrototype.ID}");

                    Assert.That(woundSystem.GetWoundableSeverityPoint(parentPart.Value), Is.GreaterThanOrEqualTo(FixedPoint2.Zero), $"Failed species to pass the test: {speciesPrototype.ID}");
                    Assert.That(woundSystem.GetWoundableSeverityPoint(groinEntity.Id), Is.GreaterThanOrEqualTo(FixedPoint2.Zero), $"Failed species to pass the test: {speciesPrototype.ID}");

                    Assert.That(consciousnessSystem.CheckConscious(dummy), $"Failed species to pass the test: {speciesPrototype.ID}");
                });
            }
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task AllSpeciesHaveValidWoundables()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true,
            Connected = true,
            InLobby = false,
        });

        var server = pair.Server;

        var entMan = server.ResolveDependency<IEntityManager>();
        var bodySystem = entMan.System<BodySystem>();

        await server.WaitAssertion(() =>
        {
            foreach (var speciesPrototype in server.ProtoMan.EnumeratePrototypes<SpeciesPrototype>())
            {
                if (_ignoredPrototypes.Contains(speciesPrototype.ID))
                    continue;

                var dummy = entMan.Spawn(speciesPrototype.Prototype);
                foreach (var bodyPart in bodySystem.GetBodyChildren(dummy))
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(entMan.HasComponent<NerveComponent>(bodyPart.Id));
                        Assert.That(entMan.TryGetComponent(bodyPart.Id, out WoundableComponent woundable));

                        var bone = woundable.Bone.ContainedEntities.FirstOrNull();
                        Assert.That(bone, Is.Not.Null);
                        Assert.That(entMan.HasComponent<BoneComponent>(bone));
                    });
                }
            }
        });

        await pair.CleanReturnAsync();
    }
}
