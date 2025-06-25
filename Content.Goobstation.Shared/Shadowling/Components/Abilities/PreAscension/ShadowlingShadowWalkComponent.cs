using Robust.Shared.Audio;
using Robust.Shared.GameStates;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for Shadow Walk ability. Will also be used on Lesser Shadowlings.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingShadowWalkComponent : Component
{
    public string? ActionShadowWalk = "ActionShadowWalk";

    [DataField]
    public bool IsActive;

    [DataField]
    public float WalkSpeedModifier = 1.5f;

    [DataField]
    public float RunSpeedModifier = 1.5f;

    [DataField]
    public float TimeUntilDeactivation = 10f;

    [DataField]
    public float EffectOutTimer = 0.6f;

    [DataField]
    public float Timer;

    [DataField]
    public string? ShadowWalkEffectIn = "ShadowlingShadowWalkInEffect";

    [DataField]
    public string? ShadowWalkEffectOut = "ShadowlingShadowWalkOutEffect";

    [DataField]
    public SoundSpecifier? ShadowWalkSound = new SoundPathSpecifier("/Audio/Effects/bamf.ogg");
}
