using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// Allows you to spawn an entity in each cardinal direction of the spawning entity.
/// Optionally, can have them be attached to the entity itself.
/// </summary>

[RegisterComponent]
public sealed partial class CardinalSpawnerComponent : Component
{
    /// <summary>
    /// Who to attach the spawned entities to.
    /// </summary>
    [DataField]
    public EntityUid Owner;

    /// <summary>
    /// If the entities spawned should be attached to the spawner entity or not.
    /// </summary>
    [DataField]
    public bool SpawnAttached;

    /// <summary>
    /// How far away to spawn from the spawner entity. Will achieve a circle around the entity.
    /// </summary>
    [DataField]
    public float Offset = 3f;

    /// <summary>
    /// A combo of the direection to spawn, and which entity to spawn in that direction.
    /// </summary>
    [DataField]
    public Dictionary<SpawnDirection, EntProtoId> Directions = new Dictionary<SpawnDirection, EntProtoId>();

    /// <summary>
    /// Will spawn in all possible directions.
    /// </summary>
    [DataField]
    public bool AllDirections;

    /// <summary>
    /// Default to this prototype so that it only spawns this one, prevents you from having to fill in each prototype field.
    /// </summary>
    [DataField]
    public EntProtoId? StandardPrototype;
}

/// <summary>
/// Which directions to spawn the prototypes in.
/// </summary>
public enum SpawnDirection : byte
{
    North,
    South,
    East,
    West,
    Northeast,
    Northwest,
    Southeast,
    Southwest
}
