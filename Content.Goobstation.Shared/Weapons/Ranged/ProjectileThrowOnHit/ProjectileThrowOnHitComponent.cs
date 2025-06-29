// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Weapons.Ranged.ProjectileThrowOnHit;

/// <summary>
/// This is used for a projectile that tosses entities it hits.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ProjectileThrowOnHitComponent : Component
{
    /// <summary>
    /// The speed at which hit entities should be thrown.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Speed = 10f;

    /// <summary>
    /// The maximum distance the hit entity should be thrown.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Distance = 20f;

    /// <summary>
    /// Whether or not anchorable entities should be unanchored when hit.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool UnanchorOnHit;

    /// <summary>
    /// How long should this stun the target, if applicable?
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan? StunTime;
}

/// <summary>
/// Raised a weapon entity with <see cref="ProjectileThrowOnHitComponent"/> to see if a throw is allowed.
/// </summary>
[ByRefEvent]
public record struct AttemptProjectileThrowOnHitEvent(EntityUid Target, EntityUid? User, bool Cancelled = false, bool Handled = false);

/// <summary>
/// Raised a target entity before it is thrown by <see cref="ProjectileThrowOnHitComponent"/>.
/// </summary>
[ByRefEvent]
public record struct ProjectileThrowOnHitStartEvent(EntityUid Weapon, EntityUid? User);
