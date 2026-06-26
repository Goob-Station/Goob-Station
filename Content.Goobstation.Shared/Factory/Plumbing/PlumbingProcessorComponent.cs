namespace Content.Goobstation.Shared.Factory.Plumbing;

/// <summary>
/// Marks a plumbing machine as a processor in a liquid transfer chain.
/// Processors may constrain transfer speed, filter reagents, or modify the solution in transit.
/// </summary>
[RegisterComponent]
public sealed partial class PlumbingProcessorComponent : Component
{
}
