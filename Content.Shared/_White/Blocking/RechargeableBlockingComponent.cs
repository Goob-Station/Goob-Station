// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._White.Blocking;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RechargeableBlockingComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float DischargedRechargeRate = 2f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float ChargedRechargeRate = 3f;

    [ViewVariables, AutoNetworkedField]
    public bool Discharged;
}
