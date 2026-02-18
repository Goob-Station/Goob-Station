using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Spawners;

/// <summary>
/// Spawns a table of entities on despawn.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpawnTableOnDespawnComponent : Component
{
    [DataField(required: true)]
    public EntityTableSelector Table;
}
