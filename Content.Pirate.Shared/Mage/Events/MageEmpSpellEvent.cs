using Content.Shared.Actions;
using Content.Shared.Chat;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Magic;

namespace Content.Pirate.Shared.Mage.Events;

public sealed partial class MageEmpSpellEvent : InstantActionEvent//, ISpeakSpell
{

    //[DataField("speech")]
    //public string? Speech { get; set;}

    /// <summary>
    /// How much mana should be drained.
    /// </summary>
    [DataField("manaCost")]
    public float ManaCost = 20f;

    /// <summary>
    /// Range of the EMP in tiles.
    /// </summary>
    [DataField("empRange")]
    public float EmpRange = 6f;

    /// <summary>
    /// Power consumed from batteries by the EMP
    /// </summary>
    [DataField("empConsumption")]
    public float EmpConsumption = 100000f;

    /// <summary>
    /// How long the EMP effects last for, in seconds
    /// </summary>
    [DataField("empDuration")]
    public float EmpDuration = 20f;

    //public InGameICChatType ChatType { get; } = InGameICChatType.Speak;
}
