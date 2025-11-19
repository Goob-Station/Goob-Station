// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;

namespace Content.Server.StationEvents.Events;

public sealed class KudzuGrowthRule : StationEventSystem<KudzuGrowthRuleComponent>
{
    protected override void Started(EntityUid uid, KudzuGrowthRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        // Pick a place to plant the kudzu.
        if (!TryFindRandomTile(out var targetTile, out _, out var targetGrid, out var targetCoords))
            return;
        Spawn("Kudzu", targetCoords);
        Sawmill.Info($"Spawning a Kudzu at {targetTile} on {targetGrid}");

    }
}
