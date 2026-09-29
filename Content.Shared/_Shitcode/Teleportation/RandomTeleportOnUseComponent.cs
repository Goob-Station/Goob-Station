// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Trigger.Components.Effects;
using Robust.Shared.GameStates;

namespace Content.Shared.Teleportation;

/// <summary>
///     Entity that will randomly teleport the user when used in hand.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class RandomTeleportOnUseComponent : RandomTeleportComponent
{
    /// <summary>
    ///     Whether to consume this item on use; consumes only one if it's a stack
    /// </summary>
    [DataField]
    public bool ConsumeOnUse = true;

    /// <summary>
    /// Whether or not to teleport body part on use
    /// </summary>
    [DataField]
    public bool CanTeleportBodyParts = false;

    /// <summary>
    /// If <see cref="CanTeleportBodyParts"/> is true, how big is the chance of teleporting body parts
    /// Change this if you want less incident
    /// </summary>
    [DataField]
    public float BodyPartTeleportChance = 1f;

    /// <summary>
    /// Determine which body part is chosen, organ or limb.
    /// </summary>
    [DataField]
    public float OrganOrLimb = 0.5f;

    /// <summary>
    /// Determine the chance of picking the unfortunate organ if <see cref="OrganOrPart"/> goes to be organ
    /// </summary>
    [DataField]
    public float OrganPickerChance = 0.5f;

    /// <summary>
    /// Determine the chance of picking the unfortunate limb if <see cref="OrganOrPart"/> goes to be limb
    /// </summary>
    [DataField]
    public float LimbPickerChance = 0.5f;
}
