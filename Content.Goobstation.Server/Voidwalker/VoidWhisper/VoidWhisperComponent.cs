namespace Content.Goobstation.Server.Voidwalker.VoidWhisper;

/// <summary>
/// This is used for the Void Whisper action
/// </summary>
[RegisterComponent]
public sealed partial class VoidWhisperComponent : Component
{
    /// <summary>
    /// Should we apply the void accent to this whisper?
    /// </summary>
    [DataField]
    public bool ApplyAccent = true;

    [DataField]
    public LocId DialogueTitle = "void-whisper-title";

    [DataField]
    public LocId WhisperTitle = "void-whisper-whisper";

    [DataField]
    public LocId PopupTitle = "void-whisper-popup";
}
