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
        "ActionCreateRevivalContract",
    };

    /// <summary>
    /// The amount of souls or successful contracts the entity has.
    /// </summary>
    [DataField]
    public int Souls = 0;

    /// <summary>
    /// The true name of the devil.
    /// This is auto-generated from a list in the system.
    /// </summary>
    [DataField]
    public string TrueName;

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
}
