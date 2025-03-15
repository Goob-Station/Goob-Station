using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.NTR;

/// <summary>
/// A data structure for storing currently available NTR bounties.
/// </summary>
[DataDefinition, NetSerializable, Serializable]
public readonly partial record struct NTRBountyData
{
    /// <summary>
    /// A unique id used to identify the bounty
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The prototype containing information about the bounty.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField(required: true)]
    public ProtoId<NTRBountyPrototype> Bounty { get; init; } = string.Empty;

    public NTRBountyData(NTRBountyPrototype bounty, int uniqueIdentifier)
    {
        Bounty = bounty.ID;
        Id = $"{bounty.IdPrefix}{uniqueIdentifier:D3}";
    }
}
