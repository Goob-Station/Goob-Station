// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Weapons.Ranged.ProjectileRequiresWhitelist;

/// <summary>
/// Allows a projectile to only hit entities on a whitelist.
/// </summary>
[RegisterComponent]
[Access(typeof(ProjectileRequireWhitelistSystem))]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ProjectileRequireWhitelistComponent : Component
{
    /// <summary>
    /// The whitelist for what the projectile can affect.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Should the projectile hit walls?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool CollideWithWalls = true;

    /// <summary>
    /// Whether the whitelist should be inverted.
    /// </summary>
    /// <remarks>
    /// If this is true, and the whitelist is set to clumsy, the projectile will affect anyone that does *not* have the clumsy component.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public bool Invert;
}
