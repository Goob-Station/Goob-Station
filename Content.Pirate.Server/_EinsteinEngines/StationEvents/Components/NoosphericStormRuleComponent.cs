using Content.Pirate.Server.StationEvents.Events;

namespace Content.Pirate.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(NoosphericStormRule))]
public sealed partial class NoosphericStormRuleComponent : Component
{
    /// <summary>
    /// How many potential psionics should be awakened at most.
    /// </summary>
    [DataField("maxAwaken")]
    public int MaxAwaken = 3;

    /// <summary>
    /// </summary>
    [DataField("baseGlimmerAddMin")]
    public int BaseGlimmerAddMin = 65;

    /// <summary>
    /// </summary>
    [DataField("baseGlimmerAddMax")]
    public int BaseGlimmerAddMax = 85;

    /// <summary>
    /// Multiply the EventSeverityModifier by this to determine how much extra glimmer to add.
    /// </summary>
    [DataField("glimmerSeverityCoefficient")]
    public float GlimmerSeverityCoefficient = 0.25f;
}
