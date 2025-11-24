using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Grants the Slasher an area stagger action that slows nearby enemies briefly.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlasherStaggerAreaComponent : Component
{
    [ViewVariables]
    public EntityUid? ActionEnt;

    [DataField]
    public EntProtoId ActionId = "ActionSlasherStaggerArea";

    /// <summary>
    /// Radius of the aura effect.
    /// </summary>
    [DataField]
    public float Range = 3.5f;

    /// <summary>
    /// Duration of the slowdown.
    /// </summary>
    [DataField]
    public float SlowDuration = 8f;

    /// <summary>
    /// Speed debuff.
    /// </summary>
    [DataField]
    public float SlowMultiplier = 0.5f;

    /// <summary>
    /// Sound to play when the stagger area is activated.
    /// </summary>
    [DataField]
    public SoundSpecifier StaggerSound = new SoundPathSpecifier("/Audio/_Goobstation/Effects/Slasher/SlasherStaggerArea.ogg")
    {
        Params = AudioParams.Default
                       .WithMaxDistance(4f)
    };
}
