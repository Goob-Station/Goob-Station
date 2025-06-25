using Robust.Shared.Audio;
using Robust.Shared.GameStates;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for the Rapid Re-Hatch ability
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingRapidRehatchComponent : Component
{
    public string? ActionRapidRehatch = "ActionRapidRehatch";

    public EntityUid? ActionRapidRehatchEntity { get; set; }

    [DataField]
    public float DoAfterTime = 4f;

    [DataField]
    public string? RapidRehatchEffect = "ShadowlingRapidRehatchEffect";

    [DataField]
    public SoundSpecifier? RapidRehatchSound = new SoundPathSpecifier("/Audio/_EinsteinEngines/Shadowling/rapid_rehatch.ogg");
}
