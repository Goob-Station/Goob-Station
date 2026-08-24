// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._EinsteinEngines.TelescopicBaton;

[RegisterComponent, NetworkedComponent]
public sealed partial class KnockdownOnHitComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(1);

    [DataField]
    public bool DropItems = false;

    [DataField]
    public bool Autostand = true;

    [DataField]
    public bool RefreshDuration = true;

    [DataField]
    public bool KnockdownOnHeavyAttack = true; // Goobstation
}
