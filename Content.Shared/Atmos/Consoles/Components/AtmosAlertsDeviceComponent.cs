// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Atmos.Components;

[RegisterComponent, NetworkedComponent]
[Access([])]
public sealed partial class AtmosAlertsDeviceComponent : Component
{
    /// <summary>
    /// The group that the entity belongs to
    /// </summary>
    [DataField, ViewVariables]
    public AtmosAlertsComputerGroup Group;
}