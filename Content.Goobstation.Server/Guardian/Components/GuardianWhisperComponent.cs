using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Guardian.Components;

/// <summary>
/// Make guardian able to whisper to it's owner
/// </summary>
[RegisterComponent]
public sealed partial class GuardianWhisperComponent : Component
{
    [ViewVariables]
    public EntityUid? ActionUid;

    /// <summary>
    /// ID Action for whisper
    /// </summary>
    [DataField]
    public EntProtoId GuardianWhisper = "GuardianWhisperAction";
}
