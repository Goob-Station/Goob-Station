// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Storage.Components;

/// <summary>
/// Applies an ongoing pickup area around the attached entity.
/// </summary>
[NetworkedComponent]
[RegisterComponent, AutoGenerateComponentPause, AutoGenerateComponentState]
public sealed partial class MagnetPickupComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("nextScan")]
    [AutoPausedField]
    public TimeSpan NextScan = TimeSpan.Zero;

    /// <summary>
    /// If true, ignores SlotFlags and can magnet pickup on hands/ground.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    [AutoNetworkedField]
    public bool ForcePickup = true;

    [ViewVariables(VVAccess.ReadWrite), DataField("range")]
    public float Range = 1f;
}