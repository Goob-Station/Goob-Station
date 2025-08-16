using Content.Shared.Actions;
using Content.Shared.Chat;
using Robust.Shared.Audio;
using Content.Shared.Magic;

namespace Content.Pirate.Server.Mage.Events;

public sealed partial class MageLightningSpellEvent : InstantActionEvent//, ISpeakSpell
{
    /// <summary>
    /// The range this lightning hits
    /// 4f is the default
    /// </summary>
    [DataField("maxElectrocutionRange")]
    public float MaxElectrocutionRange = 4f;

    /// <summary>
    /// The maximum amount of damage the electrocution can do
    /// scales with severity
    /// </summary>
    [DataField("maxElectrocuteDamage"), ViewVariables(VVAccess.ReadWrite)]
    public float MaxElectrocuteDamage = 10f;

    /// <summary>
    /// The maximum amount of time the electrocution lasts
    /// scales with severity
    /// </summary>
    [DataField("maxElectrocuteDuration"), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan MaxElectrocuteDuration = TimeSpan.FromSeconds(2);

    //[DataField("speech")]
    //public string? Speech { get; set; }

    /// <summary>
    /// How much mana should be drained.
    /// </summary>
    [DataField("manaCost")]
    public float ManaCost = 10f;

    //public InGameICChatType ChatType { get; } = InGameICChatType.Speak;
}
