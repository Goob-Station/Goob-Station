using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;

/// <summary>
/// This is used for Icy Veins. The freezing part of the ability has its own component and system, that latches into the targets.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingIcyVeinsComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionIcyVeins";

    [DataField]
    public EntityUid? ActionEnt;

    /// <summary>
    /// The search radius of the ability.
    /// </summary>
    [DataField]
    public float Range = 6f;

    /// <summary>
    /// Indicates how long the targets will be paralyzed for.
    /// </summary>
    [DataField]
    public float ParalyzeTime = 1f;

    /// <summary>
    /// The effect that is used once the ability activates.
    /// </summary>
    [DataField]
    public EntProtoId IcyVeinsEffect = "ShadowlingIcyVeinsEffect";

    /// <summary>
    /// The sound that plays during the ability.
    /// </summary>
    [DataField]
    public SoundSpecifier? IcyVeinsSound = new SoundPathSpecifier("/Audio/_EinsteinEngines/Effects/ghost2.ogg");
}
