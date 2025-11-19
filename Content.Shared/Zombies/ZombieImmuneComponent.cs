// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.Zombies;

/// <summary>
/// Entities with this component cannot be zombified.
/// </summary>
[RegisterComponent]
public sealed partial class ZombieImmuneComponent : Component
{
    //still no
}

