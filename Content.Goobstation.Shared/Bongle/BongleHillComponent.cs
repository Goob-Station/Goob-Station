using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Bongle;

/// <summary>
/// It's a reskinned bingle pit except it doesnt spread tiles, doesnt grow, and spawns more hills once enough points are reached.
/// </summary>
[RegisterComponent]
public sealed partial class BongleHillComponent : Component
{
    [DataField]
    public string ContainerId = "hill";

    [ViewVariables]
    public Container Container = default!;

    [ViewVariables]
    public int Points;

    [DataField]
    public int PointsToSpread = 50;

    /// <summary>
    /// Anything that doesn't fit into the other entry just gives default points.
    /// </summary>
    [DataField]
    public int DefaultPoints = 1;

    /// <summary>
    /// The first entry whose whitelist matches decides how many points the thing is worth.
    /// </summary>
    [DataField]
    public List<HillPointValue> PointValues;

    /// <summary>
    /// If not crit or dead, walking over the hill is completely fine.
    /// Because it is a hill, not a pit, duh.
    /// </summary>
    [DataField]
    public bool IncapacitatedOnly = true;

    [DataField(required: true)]
    public EntProtoId HillProto;

    [DataField(required: true)]
    public EntProtoId BongleProto;

    [DataField]
    public float SpreadMinRange = 3f;

    [DataField]
    public float SpreadMaxRange = 6f;

    /// <summary>
    /// Dead bingles. Gotta pass the whitelist and also not be in the blacklist or else this wont work.
    /// </summary>
    [DataField]
    public EntityWhitelist? TriumphWhitelist;

    [DataField]
    public EntityWhitelist? TriumphBlacklist;

    /// <summary>
    /// Keeps track of how many bingle corpses this hill has slurped up.
    /// </summary>
    [ViewVariables]
    public int BingleCorpses;

    /// <summary>
    /// Triumph over the adversary increases mouth capacity and spit count.
    /// </summary>
    [DataField]
    public int CorpsesForTriumph = 3;

    [DataField]
    public int CapacityIncrease = 1;

    [DataField]
    public int SpitIncrease = 1;

    /// <summary>
    /// Numbers of hills needed to spawn before bongles ascend.
    /// </summary>
    [DataField]
    public int AscensionHillCount = 5;

    [DataField]
    public SoundSpecifier SuckedInSound = new SoundPathSpecifier("/Audio/_EinsteinEngines/Voice/Slime/slime_schlorp.ogg");

    [DataField]
    public SoundSpecifier? AscensionSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/Ambience/Antag/Heretic/ascend_ash.ogg");

    [DataField]
    public Color AnnouncementColor = Color.Pink;
}

[DataDefinition]
public sealed partial class HillPointValue
{
    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();

    [DataField]
    public int Points = 1;
}
