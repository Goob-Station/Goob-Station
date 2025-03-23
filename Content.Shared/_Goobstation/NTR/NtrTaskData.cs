using Robust.Shared.Serialization;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.NTR;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.NTR;

/// <summary>
/// A data structure for storing currently available bounties.
/// </summary>
[DataDefinition, NetSerializable, Serializable]
public readonly partial record struct NtrTaskData
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
    public ProtoId<NtrTaskPrototype> Task { get; init; } = string.Empty;

    public NtrTaskData(NtrTaskPrototype task, int uniqueIdentifier)
    {
        Task = task.ID;
        Id = $"{task.IdPrefix}{uniqueIdentifier:D3}";
    }
}
