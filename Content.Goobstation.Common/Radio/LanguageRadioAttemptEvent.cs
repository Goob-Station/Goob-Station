namespace Content.Goobstation.Common.Radio;

[ByRefEvent]
public record struct LanguageRadioAttemptEvent(string Language)
{
    public bool Allowed;
    public string? TranslateTo;
}
