using Content.Shared.Psionics;
using Robust.Shared.Serialization;

/// <summary>
/// EVERYTHING HERE IS A MODIFIED VERSION OF CRIMINAL RECORDS
/// </summary>

namespace Content.Shared.PsionicsRecords;

/// <summary>
/// Psionics record for a crewmember.
/// Can be viewed and edited in a psionics records console by epistemics.
/// </summary>
[Serializable, NetSerializable, DataRecord]
public sealed record PsionicsRecord
{
    /// <summary>
    /// Status of the person (None, Suspect, Registered, Abusing).
    /// </summary>
    [DataField]
    public PsionicsStatus Status = PsionicsStatus.None;

    /// <summary>
    /// When Status is Anything but none, the reason for it.
    /// Should never be set otherwise.
    /// </summary>
    [DataField]
    public string? Reason;
}
