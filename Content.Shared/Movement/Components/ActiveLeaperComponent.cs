// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

/// <summary>
/// Marker component given to the users of the <see cref="JumpAbilityComponent"/> if they are meant to collide with environment.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ActiveLeaperComponent : Component
{
    /// <summary>
    /// The duration to stun the owner on collide with environment.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan KnockdownDuration;
}
