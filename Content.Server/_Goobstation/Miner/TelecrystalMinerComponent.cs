using Robust.Shared.Map;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Audio;

namespace Content.Server._Goobstation.Miner;

[RegisterComponent]
public sealed partial class TelecrystalMinerComponent : Component
{
    /// <summary>
    /// How much TC does this thing have in its buffer
    /// </summary>
    [DataField("accumulatedTC")]
    public float AccumulatedTC = 0f;

    /// <summary>
    /// Was there a CC announcement after 10 mins
    /// </summary>
    [DataField("notified")]
    public bool Notified = false;

    /// <summary>
    /// Time since last update
    /// </summary>
    [DataField("lastUpdate", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan LastUpdate = TimeSpan.Zero;

    /// <summary>
    /// Miner working SFX
    /// </summary>
    [DataField("miningSound")]
    public SoundSpecifier MiningSound = new SoundPathSpecifier("/Audio/Ambience/Objects/server_fans.ogg");

    /// <summary>
    /// Counts time to know when to make CC announcement.
    /// </summary>
    [DataField("startTime")]
    public TimeSpan? StartTime { get; set; } = null;

    /// <summary>
    /// How often the miner produces TC (in seconds)
    /// </summary>
    [DataField("miningInterval")]
    public float MiningInterval = 10.0f;

    /// <summary>
    /// Power consumption
    /// </summary>
    [DataField("powerDraw")]
    public float PowerDraw = 10000f;

    /// <summary>
    ///     Origin map and grid of this [MINER].
    ///     If a station wasn't tied to a given grid when the bomb was spawned,
    ///     this will be filled in instead.
    /// </summary>
    public (MapId, EntityUid?)? OriginMapGrid; // totally not shitcode from nukecomponent btw

    /// <summary>
    ///     Origin station of this [MINER], if it exists.
    ///     If this doesn't exist, then the origin grid and map will be filled in, instead.
    /// </summary>
    public EntityUid? OriginStation;
}
