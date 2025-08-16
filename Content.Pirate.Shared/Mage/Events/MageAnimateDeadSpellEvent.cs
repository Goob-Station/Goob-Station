using Content.Shared.Actions;
using Content.Shared.Chat;
using Robust.Shared.Audio;
using Content.Shared.Magic;

namespace Content.Pirate.Shared.Mage.Events;

public sealed partial class MageAnimateDeadSpellEvent : InstantActionEvent//, ISpeakSpell
{
    /// <summary>
    /// The range this spell opens doors in
    /// 4f is the default
    /// </summary>
    [DataField("range")]
    public float Range = 4f;

    [DataField("sound")]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Magic/staff_healing.ogg");

    /// <summary>
    /// Volume control for the spell.
    /// </summary>
    [DataField("volume")]
    public float Volume = 5f;

    [DataField("speech")]
    public string? Speech { get; private set; }

    /// <summary>
    /// How much mana should be drained.
    /// </summary>
    [DataField("manaCost")]
    public float ManaCost = 20f;

    //public InGameICChatType ChatType { get; } = InGameICChatType.Speak;
}
