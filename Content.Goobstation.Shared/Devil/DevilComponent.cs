using Content.Shared.Dataset;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Devil;

[RegisterComponent]
public sealed partial class DevilComponent : Component
{
    public readonly List<EntProtoId> BaseDevilActions = new()
    {
        "ActionCreateContract",
        "ActionShadowJaunt",
    };

    /// <summary>
    /// The amount of souls or successful contracts the entity has.
    /// </summary>
    [DataField]
    public int Souls = 0;

    /// <summary>
    /// Should it perform startup tasks and apply items?
    /// </summary>
    /// <remarks>
    /// False by default so possession doesn't bork.
    /// Run the gamerule if you want to make someone a devil silly.
    /// </remarks>
    public bool DoStartup = false;

    /// <summary>
    /// The true name of the devil.
    /// This is auto-generated from a list in the system.
    /// </summary>
    [DataField]
    public string TrueName;

    /// <summary>
    /// The current power level of the devil.
    /// </summary>
    [DataField]
    public int PowerLevel = 0;

    /// <summary>
    /// Sound effect played when summoning a contract.
    /// </summary>
    [DataField]
    public SoundPathSpecifier FwooshPath = new SoundPathSpecifier("/Audio/_Goobstation/Effects/fwoosh.ogg");

    /// <summary>
    /// When the true-name stun was last triggered
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastTriggeredTime;

    /// <summary>
    /// Minimum time between true-name triggers
    /// </summary>
    [DataField]
    public TimeSpan CooldownDuration = TimeSpan.FromSeconds(30);

    [DataField]
    public ProtoId<DatasetPrototype> FirstNameTrue = new("names_devil_first");

    [DataField]
    public ProtoId<DatasetPrototype> LastNameTrue = new("names_devil_last");


}
