// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.OfficeChair;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VehicleHitAndRunComponent : Component
{
    /// <summary>
    /// Minimum vehicle speed required to launch people.
    /// </summary>
    [DataField] public float MinRunoverSpeed = 3f;

    /// <summary>
    /// Base throw speed added to the scaled relative velocity when launching a target.
    /// </summary>
    [DataField] public float LaunchForceBase = 15f;

    /// <summary>
    /// Multiplier applied to the relative speed between vehicle and target to compute throw speed.
    /// </summary>
    [DataField] public float LaunchForceScale = 1.5f;

    /// <summary>
    /// Per target cooldown in seconds before they can be launched again.
    /// </summary>
    [DataField] public float ThrowCooldown = 0.6f;

    /// <summary>
    /// Radius from the entity in which people get hit.
    /// </summary>
    [DataField] public float RunoverRadius = 1f;

    /// <summary>
    /// Time in seconds the thrown entity remains airborne; used to determine travel distance.
    /// </summary>
    [DataField] public float AirTime = 0.6f;

    /// <summary>
    /// Sound collection played when at least one entity is launched
    /// </summary>
    [DataField] public SoundCollectionSpecifier RunOverSound = new SoundCollectionSpecifier("ClowncarCrash");

    [DataField, AutoNetworkedField] public bool CanRunOver = true;

    public Dictionary<EntityUid, TimeSpan> LastLaunched = new();
}
