namespace Content.Shared.Security;

/// <summary>
/// Status used in Criminal Records.
///
/// None - the default value
/// Suspected - the person is suspected of doing something illegal
/// Wanted - the person is being wanted by security
/// Detained - the person is detained by security
/// Paroled - the person is on parole
/// Discharged - the person has been released from prison
/// Search - the person needs to be searched
/// Perma - the person has been sentenced to permanent imprisonment
/// Dangerous - the person is highly dangerous and may resist arrest
/// </summary>
public enum SecurityStatus : byte
{
    None,
    Suspected,
    Wanted,
    Detained,
    Paroled,
    Discharged,
    Search,
    Perma,
    Dangerous
}
