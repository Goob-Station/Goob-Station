// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Construction.Completions;
using Content.Server.StationEvents.Events;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.StationEvents;

public sealed class AddComponentRandomRule : StationEventSystem<AddComponentRandomRuleComponent>
{
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    protected override void Started(EntityUid uid, AddComponentRandomRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        var eligibleEntities = new List<EntityUid>();
        var chosenEntities = new List<EntityUid>();

        var query = EntityQueryEnumerator<MindContainerComponent>();
        while (query.MoveNext(out var target, out var mindContainer))
        {
            if (mindContainer.Mind == null
                || _entityWhitelist.IsValid(component.Blacklist, target)
                || !_mobState.IsAlive(target))
                continue;

            eligibleEntities.Add(target);
        }

        var amount = component.Amount;
        for (var i = 0; i < amount; i++)
        {
            if (eligibleEntities.Count == 0)
                break;

            chosenEntities.Add(_random.PickAndTake(eligibleEntities));
        }

        foreach (var entity in chosenEntities)
        {
            foreach (var comp in component.Components)
            {
                EntityManager.AddComponent(entity, comp.Value);
                Log.Debug($"Added {comp.Value} to: {ToPrettyString(entity)}.");
                _adminLogger.Add(LogType.AntagSelection, $"Added {comp} to: {ToPrettyString(entity)}.");
            }
        }
    }
}
