using Content.Goobstation.Common.VoxAudio;
using Robust.Shared.Prototypes;

namespace Content.Server.Communications;

public sealed partial class CommunicationsConsoleComponent
{
    /// <summary>
    /// Goobstation
    /// What alert level to set it to if the console is emagged.
    /// </summary>
    [DataField] public string AlertLevelOnEmag = "honk";

    /// <summary>
    /// Goobstation - Whether to play vox audio announcement.
    /// </summary>
    [DataField]
    public bool EnableVox = false;

    /// <summary>
    /// Goobstation - The set of voxVoices used when playing vox audio announcement.
    /// </summary>
    [DataField]
    public List<ProtoId<VoxVoicePrototype>> VoxVoices = [];
}