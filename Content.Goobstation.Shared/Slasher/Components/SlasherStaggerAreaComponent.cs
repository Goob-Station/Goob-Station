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
    public float SlowDuration = 4f;

    /// <summary>
    /// Speed debuff.
    /// </summary>
    [DataField]
    public float SlowMultiplier = 0.5f;

    /// <summary>
    /// Optional sound to play when activating the aura.
    /// </summary>
    [DataField]
    public SoundSpecifier? ActivateSound;
}

public sealed partial class SlasherStaggerAreaEvent : InstantActionEvent
{
}
