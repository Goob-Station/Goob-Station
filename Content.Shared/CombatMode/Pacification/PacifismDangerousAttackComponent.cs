// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.CombatMode.Pacification;

/// <summary>
/// This is used for marking entities which could cause serious harm if attacked and should not be able to be harmed by
/// pacifists.
/// TODO ideally destructible is shared + converted to components so we can just check for a harmful damage trigger instead of this.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PacifismDangerousAttackComponent : Component
{
}