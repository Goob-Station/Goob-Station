using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.NTR;
/// <summary>
/// This is a prototype for a cargo bounty, a set of items
/// that must be sold together in a labeled container in order
/// to receive a monetary reward.
/// </summary>
[Prototype, Serializable, NetSerializable]
public sealed class NtrTaskPrototype : IPrototype
{
    [DataField]
    public string Proto { get; init; } = default!;

    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The monetary reward for completing the bounty
    /// </summary>
    [DataField(required: true)]
    public int Reward;

    /// <summary>
    /// A description for flava purposes.
    /// </summary>
    [DataField]
    public LocId Description = string.Empty;

    /// <summary>
    /// The entries that must be satisfied for the cargo bounty to be complete.
    /// </summary>
    [DataField(required: true)]
    public List<NtrTaskItemEntry> Entries = new();

    /// <summary>
    /// A prefix appended to the beginning of a bounty's ID.
    /// </summary>
    [DataField]
    public string IdPrefix = "CC";

    /// <summary>
    /// Weight for random selection (higher = more frequent)
    /// </summary>
    [DataField]
    public float Weight = 1.0f;
}

[DataDefinition, Serializable, NetSerializable]
public readonly partial record struct NtrTaskItemEntry()
{
    [DataField]
    public List<string> Stamps { get; init; } = default!;

    /// <summary>
    /// A whitelist for determining what items satisfy the entry.
    /// </summary>
    [DataField]
    public EntityWhitelist Whitelist { get; init; } = default!;

    /// <summary>
    /// How much of the item must be present to satisfy the entry
    /// </summary>
    [DataField]
    public int Amount { get; init; } = 1;
    [DataField]
    public bool InstantCompletion { get; init; } = false;

    [DataField]
    public bool IsEvent { get; init; } = false;

    /// <summary>
    /// A player-facing name for the item.
    /// </summary>
    [DataField]
    public LocId Name { get; init; } = string.Empty;
}
