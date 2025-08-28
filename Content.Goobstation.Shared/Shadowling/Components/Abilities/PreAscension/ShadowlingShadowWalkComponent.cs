using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;

/// <summary>
/// This is used for Shadow Walk ability. Will also be used on Lesser Shadowlings.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingShadowWalkComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionShadowWalk";

    [DataField]
    public EntityUid? ActionEnt;

    /// <summary>
    /// Indicates whether the ability is active, or not.
    /// </summary>
    [DataField]
    public bool IsActive;

    /// <summary>
    /// Indicates the new walking speed of the ability user.
    /// </summary>
    [DataField]
    public float WalkSpeedModifier = 1.5f;

    /// <summary>
    /// Indicates the new running speed of the ability user.
    /// </summary>
    [DataField]
    public float RunSpeedModifier = 1.5f;

    /// <summary>
    /// Indicates how long it lasts once deactivated.
    /// </summary>
    [DataField]
    public TimeSpan TimeUntilDeactivation = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Indicates when the effect should activate.
    /// It is advised to not change this because it is perfectly timed.
    /// </summary>
    [DataField]
    public TimeSpan EffectOutTimer = TimeSpan.FromSeconds(0.6);

    [DataField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    /// Avoids duplicate effects.
    /// </summary>
    [DataField]
    public bool EffectActivated;

    /// <summary>
    /// The prototype of the effect once you use the ability
    /// </summary>
    [DataField]
    public EntProtoId ShadowWalkEffectIn = "ShadowlingShadowWalkInEffect";

    /// <summary>
    /// The prototype of the effect once you go out of the ability
    /// </summary>
    [DataField]
    public EntProtoId ShadowWalkEffectOut = "ShadowlingShadowWalkOutEffect";

    /// <summary>
    /// The sound that plays during the ability
    /// </summary>
    [DataField]
    public SoundSpecifier? ShadowWalkSound = new SoundPathSpecifier("/Audio/_EinsteinEngines/Effects/bamf.ogg");
}
