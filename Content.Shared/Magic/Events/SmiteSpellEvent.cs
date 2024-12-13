using Content.Shared.Actions;
using Content.Shared.Chat;

namespace Content.Shared.Magic.Events;

public sealed partial class SmiteSpellEvent : EntityTargetActionEvent, ISpeakSpell
{
    // TODO: Make part of gib method
    /// <summary>
    /// Should this smite delete all parts/mechanisms gibbed except for the brain?
    /// </summary>
    [DataField]
    public bool DeleteNonBrainParts = true;

    [DataField]
    public string? Speech { get; private set; }

    public InGameICChatType ChatType { get; } = InGameICChatType.Speak;
}
