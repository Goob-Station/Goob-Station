using Content.Shared.Actions;
using Content.Shared.Chat;

namespace Content.Shared.Magic.Events;

// TODO: Can probably just be an entity or something
public sealed partial class TeleportSpellEvent : WorldTargetActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    public InGameICChatType ChatType { get; } = InGameICChatType.Speak;

    // TODO: Move to magic component
    // TODO: Maybe not since sound specifier is a thing
    // Keep here to remind what the volume was set as
    /// <summary>
    /// Volume control for the spell.
    /// </summary>
    [DataField]
    public float BlinkVolume = 5f;
}
