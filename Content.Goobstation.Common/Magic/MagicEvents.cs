namespace Content.Goobstation.Common.Magic;

/// <summary>
/// Used to see if the target is valid for a mindswap.
/// </summary>
/// <param name="Message"> The text to be displayed on mindswap fail. </param>
[ByRefEvent]
public record struct BeforeMindSwappedEvent(
    bool Cancelled,
    string Message);
