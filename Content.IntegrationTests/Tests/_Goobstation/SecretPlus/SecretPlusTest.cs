// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.StationEvents.Components;
using Content.Goobstation.Server.StationEvents.SecretPlus;
using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Shared.EntityTable;
using Content.Shared.GameTicking.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using System.Collections.Generic;
using System.Linq;

namespace Content.IntegrationTests.Tests._Goobstation.SecretPlus;

[TestFixture]
public sealed class SecretPlusTest
{
    [Test]
    public async Task ValidateChaosScores()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var compFac = server.ResolveDependency<IComponentFactory>();
        var entMan = server.ResolveDependency<IEntityManager>();

        var antagSelection = entMan.System<AntagSelectionSystem>();
        var entTable = entMan.System<EntityTableSystem>();

        await server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                var protos = protoMan.EnumeratePrototypes<EntityPrototype>()
                    .Where(p => !p.Abstract)
                    .ToList();

                var checkedEvs = new HashSet<EntProtoId>();

                foreach (var proto in protos)
                {
                    if (!proto.TryGetComponent<SecretPlusComponent>(out var secretComp, compFac))
                        continue;
                    if (!proto.TryGetComponent<SelectedGameRulesComponent>(out var selectedComp, compFac))
                        continue;

                    foreach (var ev in entTable.GetSpawns(selectedComp.ScheduledGameRules))
                        CheckEvent(ev);

                    foreach (var (ev, _) in protoMan.Index(secretComp.RoundStartAntagsWeightTable).Weights)
                        CheckEvent(ev);

                    foreach (var (ev, _) in protoMan.Index(secretComp.PrimaryAntagsWeightTable).Weights)
                        CheckEvent(ev);

                    void CheckEvent(EntProtoId id)
                    {
                        if (!checkedEvs.Add(id))
                            return;

                        var evProto = protoMan.Index(id);

                        evProto.TryGetComponent<GameRuleComponent>(out var ruleComp, compFac);

                        Assert.That(ruleComp, Is.Not.Null, $"Entity prototype {id} is used as gamerule by {proto.ID}, but has no GameRuleComponent!");

                        bool any = false;
                        if (evProto.TryGetComponent<AntagSelectionComponent>(out var selection, compFac))
                        {
                            any = selection.Definitions.Any(def => def.ChaosScore != null);
                            if (any)
                                Assert.That(selection.Definitions.All(def => def.ChaosScore != null), Is.True, $"Gamerule {id} is fireable by {proto.ID}, but only some of its antag selection definitions have a choas score!");
                        }

                        // allow null chaos score if we have non-null chaos score on antag selections
                        if (!any)
                            Assert.That(ruleComp.ChaosScore, Is.Not.Null, $"Gamerule {id} is fireable by {proto.ID}, but has no chaos score!");
                    }
                }
            });
        });

        await pair.CleanReturnAsync();
    }
}
