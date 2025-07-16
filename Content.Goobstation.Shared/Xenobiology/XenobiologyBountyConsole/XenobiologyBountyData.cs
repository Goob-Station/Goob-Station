using Robust.Shared.Serialization;
using Content.Shared.Cargo.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Xenobiology.XenobiologyBountyConsole;

/// <summary>
/// A data structure for storing currently available bounties.
/// </summary>
[DataDefinition, NetSerializable, Serializable]
public readonly partial record struct XenobiologyBountyData
{
    /// <summary>
    /// A unique id used to identify the bounty
    /// </summary>
    [DataField]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The prototype containing information about the bounty.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<XenobiologyBountyPrototype> Bounty { get; init; } = string.Empty;

    public XenobiologyBountyData(XenobiologyBountyPrototype bounty, int uniqueIdentifier)
    {
        Bounty = bounty.ID;
        Id = $"{bounty.IdPrefix}{uniqueIdentifier:D3}";
    }
}
