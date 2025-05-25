using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// This is used to mark an entity as the end point for the "relocate monument" ability. ideally there should only ever be one of these
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class MonumentMoveDestinationComponent : Component
{
    public EntityUid? Monument;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? PhaseInTimer;
}
