using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Holosign;

/// <summary>
/// A holosign projector that uses <c>LimitedCharges</c> instead of a power cell slot.
/// If there is already a sign on the clicked tile it reclaims it for a charge instead of stacking it.
/// Currently there is no spawning prediction so signs are spawned once in a container and moved out to allow prediction.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(ChargeHolosignSystem))]
public sealed partial class ChargeHolosignProjectorComponent : Component
{
    /// <summary>
    /// The entity to spawn.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId SignProto;

    /// <summary>
    /// Component on <see cref="SignProto"/> to check for duplicates.
    /// </summary>
    [DataField(required: true)]
    public string SignComponentName;

    public Type SignComponent = default!;

    /// <summary>
    /// Container to store sign entities in before they are "spawned" on use.
    /// </summary>
    [DataField]
    public string ContainerId = "signs";

    [ViewVariables]
    public Container Container = default!;
}
