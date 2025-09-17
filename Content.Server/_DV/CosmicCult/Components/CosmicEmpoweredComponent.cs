// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._DV.CosmicCult.Components;

/// <summary>
///     Component used for storing and handling the empowered cultists' speed boost.
/// </summary>
[RegisterComponent]
public sealed partial class CosmicEmpoweredSpeedComponent : Component
{
    public float SpeedBoost = 1.15f;
}
