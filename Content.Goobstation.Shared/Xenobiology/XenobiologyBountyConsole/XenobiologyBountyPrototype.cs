using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Xenobiology.XenobiologyBountyConsole;

/// <summary>
/// This is a prototype for a xenobiology bounty, an item
/// from an alien creature (Currently, Just slime extracts)
/// that can be exchanged for research points.
/// </summary>
[Prototype, Serializable, NetSerializable]
public sealed partial class XenobiologyBountyPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The points awarded after multipliers.
    /// </summary>
    [DataField(required: true)]
    public float PointsAwarded;

    /// <summary>
    /// The entries that must be satisfied for the bounty to be complete.
    /// </summary>
    [DataField(required: true)]
    public List<XenobiologyBountyItemEntry> Entries = [];

    /// <summary>
    /// A prefix appended to the beginning of a bounty's ID.
    /// </summary>
    [DataField]
    public string IdPrefix = "NT";
}

[DataDefinition, Serializable, NetSerializable]
public readonly partial record struct XenobiologyBountyItemEntry()
{
    /// <summary>
    /// A whitelist for determining what items satisfy the entry.
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist Whitelist { get; init; } = default!;

    /// <summary>
    /// A blacklist that can be used to exclude items in the whitelist.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist { get; init; } = null;

    /// <summary>
    /// How much of the item must be present to satisfy the entry
    /// </summary>
    [DataField]
    public int Amount { get; init; } = 1;

    /// <summary>
    /// A player-facing name for the item.
    /// </summary>
    [DataField]
    public LocId Name { get; init; } = string.Empty;
}
