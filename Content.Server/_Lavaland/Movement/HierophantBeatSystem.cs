// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Movement;
using Content.Shared.Alert;
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Movement;

public sealed class HierophantBeatSystem : SharedHierophantBeatSystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;

    protected override void ShowAlert(EntityUid uid, ProtoId<AlertPrototype> alertId)
        => _alertsSystem.ShowAlert(uid, alertId);

    protected override void ClearAlert(EntityUid uid, ProtoId<AlertPrototype> alertId)
        => _alertsSystem.ClearAlert(uid, alertId);
}
