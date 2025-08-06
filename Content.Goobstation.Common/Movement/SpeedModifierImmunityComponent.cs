// SPDX-FileCopyrightText: 2024 Angelo Fallaria <ba.fallaria@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Movement;

/// <summary>
///   This is used to make an entity's movement speed constant and
///   never affected by almost all movement speed modifiers.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpeedModifierImmunityComponent : Component
{
}
