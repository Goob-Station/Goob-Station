// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 coolmankid12345 <55817627+coolmankid12345@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Revolutionary;
using Robust.Shared.GameStates;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;

namespace Content.Shared.Mindshield.Components;

/// <summary>
/// If a player has a Mindshield they will get this component to prevent conversion.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedRevolutionarySystem))] //Goobstation - AutoGenerateComponentState
public sealed partial class MindShieldComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<SecurityIconPrototype> MindShieldStatusIcon = "MindShieldIcon";

    // Goobstation - revs update
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<SecurityIconPrototype> MindShieldBrokenStatusIcon = "MindShieldBrokenIcon";

    // Goobstation - uses to show headrevs with disabled ability
    [DataField, AutoNetworkedField]
    public bool Broken = false;
}