// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Damage.Components;

/// <summary>
/// Prevent the object from getting hit by projetiles unless you target the object.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(RequireProjectileTargetSystem))]
public sealed partial class RequireProjectileTargetComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Active = true;

    // Goobstation - Crawl fix
    [DataField, AutoNetworkedField]
    public bool IgnoreThrow = false;
}