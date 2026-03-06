namespace Content.Server._White.Xenomorphs.Infection;

/// <summary>
/// Used for sentient facehugger infections.
/// </summary>
[RegisterComponent]
public sealed partial class SentientInfectionComponent : Component
{
    /// <summary>
    /// The mind ID of the sentient facehugger that created this infection.
    /// This mind will be transferred to the embryo and later to the larva.
    /// </summary>
    [ViewVariables]
    public EntityUid? SourceMindId;
}
