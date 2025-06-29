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
    public EntProtoId ActionRapidRehatch = "ActionRapidRehatch";

    [ViewVariables]
    public EntityUid? ActionRapidRehatchEntity { get; set; }

    [DataField]
    public float DoAfterTime = 4f;

    [DataField]
    public EntProtoId RapidRehatchEffect = "ShadowlingRapidRehatchEffect";

    [DataField]
    public SoundSpecifier? RapidRehatchSound = new SoundPathSpecifier("/Audio/_EinsteinEngines/Shadowling/rapid_rehatch.ogg");
}
