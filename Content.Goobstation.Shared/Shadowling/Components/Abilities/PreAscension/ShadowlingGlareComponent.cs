using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;

/// <summary>
/// This is used for the Glare Ability
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingGlareComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionGlare";

    [DataField]
    public EntityUid? ActionEnt;

    // <summary>
    // Variable stun time. On distance 1 or lower, it is maximized to 4 seconds of stun (enough to Enthrall),
    // otherwise it gets reduced based on distance.
    // </summary>
    [DataField]
    public float GlareStunTime;

    // <summary>
    // Variable activation time. On distance 1 or lower, it is immediate,
    // otherwise it gets increased based on distance.
    // Max time before stun is 2 seconds
    // Note: needs to be rewritten to not use frametime
    // </summary>
    [DataField]
    public float GlareTimeBeforeEffect;

    [DataField]
    public float MaxGlareDistance = 10f;

    [DataField]
    public float MinGlareDistance = 1f;

    [DataField]
    public float MaxGlareStunTime = 7f;

    // <summary>
    // Regarding time delay before activation
    // </summary>
    [DataField]
    public float MaxGlareDelay = 2f;

    // <summary>
    // Regarding time delay before activation
    // </summary>
    [DataField]
    public float MinGlareDelay = 0.1f;

    [DataField]
    public float MuteTime = 2f;

    [DataField]
    public float SlowTime = 2f;

    [DataField]
    public EntityUid GlareTarget;

    [DataField]
    public bool ActivateGlareTimer;

    [DataField]
    public EntProtoId EffectGlare = "ShadowlingGlareEffect";
}
