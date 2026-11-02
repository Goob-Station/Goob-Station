using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.CollectiveMind;

/// <summary>
/// This is used for the Blindness Smoke ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingBlindnessSmokeComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionBlindnessSmoke";

    [DataField]
    public EntityUid? ActionEnt;

    /// <summary>
    /// The reagent used inside the smoke.
    /// </summary>
    [DataField]
    public string Reagent = "ShadowlingToxin"; // innovative name

    /// <summary>
    /// The duration of the smoke itself.
    /// </summary>
    [DataField]
    public float Duration = 5f;

    /// <summary>
    /// Indicates how much the smoke should spread in an area
    /// </summary>
    [DataField]
    public int SpreadAmount = 18;

    /// <summary>
    /// The quantity of the reagent contained inside the smoke
    /// </summary>
    [DataField]
    public FixedPoint2 ReagentQuantity = 10f;

    /// <summary>
    /// The sound used once the ability activates.
    /// </summary>
    [DataField]
    public SoundSpecifier? BlindnessSound = new SoundPathSpecifier("/Audio/_EinsteinEngines/Effects/bamf.ogg");
}
