using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;

/// <summary>
/// This is used for the Rapid Re-Hatch ability
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingRapidRehatchComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionRapidRehatch";

    [DataField]
    public EntityUid? ActionEnt;

    [ViewVariables]
    public EntityUid? ActionRapidRehatchEntity { get; set; }

    /// <summary>
    /// The default DoAfter time for the ability
    /// </summary>
    [DataField]
    public float DoAfterTime = 4f;

    /// <summary>
    /// The effect that is used once the ability completes.
    /// </summary>
    [DataField]
    public EntProtoId RapidRehatchEffect = "ShadowlingRapidRehatchEffect";

    /// <summary>
    /// The sound that plays during the ability.
    /// </summary>
    [DataField]
    public SoundSpecifier? RapidRehatchSound = new SoundPathSpecifier("/Audio/_EinsteinEngines/Shadowling/rapid_rehatch.ogg");
}
