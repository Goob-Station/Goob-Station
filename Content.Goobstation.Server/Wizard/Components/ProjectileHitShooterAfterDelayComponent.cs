// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Wizard.Components;

/// <summary>
/// Projectile with this component will set IgnoreShooter to false after a delay.
/// </summary>
[RegisterComponent]
public sealed partial class ProjectileHitShooterAfterDelayComponent : Component
{
    [DataField]
    public float Delay = 1f;
}