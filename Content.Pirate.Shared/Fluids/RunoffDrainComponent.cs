using Robust.Shared.GameStates;

namespace Content.Pirate.Shared.Fluids;

/// <summary>
/// Marks a container whose contents' liquid runoff is drained away instead of pooling on the floor.
/// Wetness overflow and washed-out / displaced stains from entities inside it (e.g. a mob being
/// washed in a washing machine) are discarded rather than spilling out around the container.
/// </summary>
[RegisterComponent]
public sealed partial class RunoffDrainComponent : Component;
