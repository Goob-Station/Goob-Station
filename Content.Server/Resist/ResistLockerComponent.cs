// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Resist;

/// <summary>
///     Goobstation - indicates that an entity CAN escape from a locker or any other container after a lengthy delay.
/// </summary>
[RegisterComponent]
[Access(typeof(ResistLockerSystem))]
public sealed partial class ResistLockerComponent : Component
{
    /// <summary>
    /// How long will this locker take to kick open, defaults to 2 minutes
    /// </summary>
    [DataField("resistTime")]
    public float ResistTime = 120f;

    /// <summary>
    /// For quick exit if the player attempts to move while already resisting
    /// </summary>
    [ViewVariables]
    public bool IsResisting = false;
}
