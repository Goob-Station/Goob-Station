using Content.Shared.Actions;
using Content.Shared.Chat;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Magic;
// using Content.Server.Magic.Events;

namespace Content.Pirate.Shared.Mage.Events;

public sealed partial class MageTeleportSpellEvent : WorldTargetActionEvent//, ISpeakSpell
{
    [DataField("blinkSound")]
    public SoundSpecifier BlinkSound = new SoundPathSpecifier("/Audio/Magic/blink.ogg");

    //[DataField("speech")]
    //public string? Speech { get; set;}

    /// <summary>
    /// Volume control for the spell.
    /// </summary>
    [DataField("blinkVolume")]
    public float BlinkVolume = 5f;

    /// <summary>
    /// How much mana should be drained.
    /// </summary>
    [DataField("manaCost")]
    public float ManaCost = 30f;

    //public InGameICChatType ChatType { get; } = InGameICChatType.Speak;
}
