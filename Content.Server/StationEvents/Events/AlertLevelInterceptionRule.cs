using Content.Server.StationEvents.Components;
using Content.Server.AlertLevel;
﻿using Content.Shared.GameTicking.Components;

namespace Content.Server.StationEvents.Events;

public sealed class AlertLevelInterceptionRule : StationEventSystem<AlertLevelInterceptionRuleComponent>
{
    [Dependency] private readonly AlertLevelSystem _alertLevelSystem = default!;

    protected override void Started(EntityUid uid, AlertLevelInterceptionRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args) // Goobstation - Changed an indent.
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var chosenStation))
            return;
        if (_alertLevelSystem.GetLevel(chosenStation.Value) != "green" && component.OverrideAlert == false) // Goobstation
            return;

        _alertLevelSystem.SetLevel(chosenStation.Value, component.AlertLevel, true, true, true, component.Locked); // Goobstation
    }
}
