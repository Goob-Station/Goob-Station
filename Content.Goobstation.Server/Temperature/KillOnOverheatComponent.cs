// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;

namespace Content.Goobstation.Server.Temperature;

/// <summary>
/// Kills an entity when its temperature goes over a threshold.
/// </summary>
[RegisterComponent, Access(typeof(KillOnOverheatSystem))]
public sealed partial class KillOnOverheatComponent : Component
{
    [DataField]
    public float OverheatThreshold = Atmospherics.T0C + 907f; // Goob edit, +110 => +907, die instantly at 1200K

    [DataField]
    public LocId OverheatPopup = "ipc-overheat-popup";
}
