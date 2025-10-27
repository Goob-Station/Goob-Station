using Content.Server._DV.CosmicCult.EntitySystems;
using Content.Shared.Maps;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._DV.CosmicCult.Components;

[RegisterComponent, Access(typeof(CosmicCorruptingSystem))]
[AutoGenerateComponentPause]
public sealed partial class CosmicCorruptingComponent : Component
{
    /// <summary>
    /// Our timer for corruption checks.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan CorruptionTimer = default!;

    /// <summary>
    /// the list of tiles that can be corrupted by this corruptor.
    /// </summary>
    [DataField]
    public HashSet<Vector2i> CorruptableTiles = [];

    /// <summary>
    /// If this corruption source can move. if true, only corrupt the immediate area around it.
    /// Slightly hacky but works for our purposes.
    /// </summary>
    [DataField]
    public bool Mobile;

    /// <summary>
    /// if this corruption source should floodfill through all corruptible tiles to initialise its corruptible tile set on activation.
    /// </summary>
    [DataField]
    public bool FloodFillStarting;

    /// <summary>
    /// How many times has this corruption source ticked?
    /// </summary>
    [DataField]
    public int CorruptionTicks;

    /// <summary>
    /// The maximum amount of ticks this source can do.
    /// </summary>
    [DataField]
    public int CorruptionMaxTicks = 50;

    /// <summary>
    /// The chance that a tile and/or wall is replaced.
    /// </summary>
    [DataField]
    public float CorruptionChance = 0.51f;

    /// <summary>
    /// The reduction applied to corruption chance every tick.
    /// </summary>
    [DataField]
    public float CorruptionReduction;

    /// <summary>
    /// Wether or not the CosmicCorruptingSystem should be running on this entity. use CosmicCorruptingSystem.Enable() instead of directly interacting with this variable.
    /// </summary>
    [DataField]
    public bool Enabled = true;

    /// <summary>
    /// Wether or not the CosmicCorruptingSystem should spawn VFX when converting tiles and walls.
    /// </summary>
    [DataField]
    public bool UseVFX = true;

    /// <summary>
    /// Wether or not the CosmicCorruptingSystem should ignore this component when it reaches max growth. Saves performance.
    /// </summary>
    [DataField]
    public bool AutoDisable = true;

    /// <summary>
    /// How much time between tile corruptions.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan CorruptionSpeed = TimeSpan.FromSeconds(6);

    /// <summary>
    /// The tile we spawn when replacing a normal tile.
    /// </summary>
    [DataField] //not a dict like the entity conversion below because there's too many fucking tiles
    public ProtoId<ContentTileDefinition> ConversionTile = "FloorCosmicCorruption";

    /// <summary>
    /// Dictionary for what entities to convert to which prototypes. Similar to CosmicCorruptibleComponent, but
    /// non-inheriting.
    /// </summary>
    [DataField]
    public Dictionary<EntProtoId, EntProtoId> EntityConversionDict = new Dictionary<EntProtoId, EntProtoId>()
    {
        {"Window", "WindowCosmicCult"},
        {"Table", "CosmicTable"},
        {"Chair", "CosmicChair"},
    };

    /// <summary>
    /// The VFX entity we spawn when corruption occurs.
    /// </summary>
    [DataField]
    public EntProtoId TileConvertVFX = "CosmicFloorSpawnVFX";

    /// <summary>
    /// The VFX entity we spawn when walls get deleted.
    /// </summary>
    [DataField]
    public EntProtoId TileDisintegrateVFX = "CosmicGenericVFX";

}
