namespace Content.Shared._EinsteinEngines.Language;

public sealed partial class SpeechOverrideInfo
{
    /// <summary>
    ///     If false, an entity can hear this language even when it's unable to see (i.e. blind),
    /// </summary>
    [DataField]
    public bool RequireSight = false;
}
