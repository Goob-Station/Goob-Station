// SPDX-FileCopyrightText: 2024 Angelo Fallaria <ba.fallaria@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Movement;

/// <summary>
///  This component is used for entities that have innate movement speed modifiers.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class InnateSpeedModifierComponent : Component
{
    [DataField, AutoNetworkedField]
    public float WalkModifier = 1.0f;

    [DataField, AutoNetworkedField]
    public float SprintModifier = 1.0f;
}
