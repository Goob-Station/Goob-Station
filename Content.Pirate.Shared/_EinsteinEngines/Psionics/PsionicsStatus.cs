/// <summary>
/// EVERYTHING HERE IS A MODIFIED VERSION OF CRIMINAL RECORDS
/// </summary>

namespace Content.Shared.Psionics;

/// <summary>
/// Status used in Psionics Records.
///
/// None - the default value
/// Suspected - the person is suspected of having psionics
/// Registered - the person is a registered psionics user
/// Abusing - the person has been caught abusing their psionics
/// </summary>
public enum PsionicsStatus : byte
{
    None,
    Suspected,
    Registered,
    Abusing
}
