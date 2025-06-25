using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for the Blindness Smoke ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingBlindnessSmokeComponent : Component
{
    public string? ActionBlindnessSmoke = "ActionBlindnessSmoke";

    [DataField]
    public string Reagent = "ShadowlingToxin"; // innovative name

    [DataField]
    public float Duration = 5f;

    [DataField]
    public int SpreadAmount = 18;

    [DataField]
    public FixedPoint2 ReagentQuantity = 10f;

    [DataField]
    public SoundSpecifier? BlindnessSound = new SoundPathSpecifier("/Audio/Effects/bamf.ogg");
}
