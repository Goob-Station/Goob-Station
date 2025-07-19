using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Shared.Actions;
using Content.Shared.Chat;

namespace Content.Pirate.Shared.BloodCult;

public sealed partial class SummonEquipmentEvent : InstantActionEvent//, ISpeakSpell PIRATE
{
    /// <summary>
    /// Slot - EntProtoId
    /// </summary>
    [DataField]
    public Dictionary<string, EntProtoId> Prototypes = new();

    [DataField]
    public string? Speech { get; set; }

    [DataField]
    public bool Force { get; set; } = true;

    [DataField]
    public InGameICChatType InvokeChatType = InGameICChatType.Whisper;

    public InGameICChatType ChatType => InGameICChatType.Whisper;
}
