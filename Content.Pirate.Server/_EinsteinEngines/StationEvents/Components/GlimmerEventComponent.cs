namespace Content.Pirate.Server.StationEvents.Components;

/// <summary>
/// Component for events that are triggered based on glimmer levels
/// </summary>
[RegisterComponent]
public sealed partial class GlimmerEventComponent : Component
{
    /// <summary>
    /// Minimum glimmer value for event to be eligible. (Should be 100 at lowest.)
    /// </summary>
    [DataField("minimumGlimmer")]
    public int MinimumGlimmer = 100;

    /// <summary>
    /// Maximum glimmer value for event to be eligible. (Remember 1000 is max glimmer period.)
    /// </summary>
    [DataField("maximumGlimmer")]
    public int MaximumGlimmer = 1000;

    /// <summary>
    /// Will be used for _random.Next and subtracted from glimmer.
    /// Lower bound.
    /// </summary>
    [DataField("glimmerBurnLower")]
    public int GlimmerBurnLower = 25;

    /// <summary>
    /// Will be used for _random.Next and subtracted from glimmer.
    /// Upper bound.
    /// </summary>
    [DataField("glimmerBurnUpper")]
    public int GlimmerBurnUpper = 70;

    [DataField("report")]
    public string SophicReport = "glimmer-event-report-generic";
}
