using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.CosmicCult.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedMonumentSystem))]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class MonumentTransformingComponent : Component
{
    /// <summary>
    /// The time when insertion ends.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField, AutoNetworkedField]
    public TimeSpan EndTime;
}
