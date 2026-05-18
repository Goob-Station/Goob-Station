using Content.Shared.EntityTable.EntitySelectors;
using Content.Shared.Trigger.Components.Effects;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Trigger;

/// <summary>
/// Spawns a protoype when triggered.
/// If TargetUser is true it will be spawned at their location.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpawnTableOnTriggerComponent : BaseXOnTriggerComponent
{
    /// <summary>
    /// The entity table to spawn.
    /// </summary>
    [DataField(required: true)]
    public EntityTableSelector Table;

    /// <summary>
    /// Use MapCoordinates for spawning?
    /// Set to true if you don't want the new entity parented to the spawner.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool UseMapCoords;

    /// <summary>
    /// Whether or not to use predicted spawning.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Predicted;
}
