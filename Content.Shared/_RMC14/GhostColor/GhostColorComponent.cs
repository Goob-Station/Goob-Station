// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.GameStates;

namespace Content.Shared._RMC14.GhostColor;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GhostColorComponent : Component
{
    [DataField, AutoNetworkedField]
    public Color? Color;
}