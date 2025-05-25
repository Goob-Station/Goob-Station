using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.NTR;

/// <summary>
/// A data structure for storing currently available bounties.
/// </summary>
[DataDefinition, NetSerializable, Serializable]
public partial record struct NtrTaskData
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

    [DataField]
    public bool IsActive = false;

    [DataField]
    public TimeSpan ActiveTime;

    [DataField]
    public bool IsCompleted = false;

    public NtrTaskData(NtrTaskPrototype task, string uniqueIdentifier)
    {
        Task = task.ID;
        Id = $"{task.IdPrefix}{uniqueIdentifier:D3}";
        IsActive = false;
        ActiveTime = TimeSpan.Zero;
    }
    public NtrTaskData AsActive(TimeSpan time)
    {
        return this with { IsActive = true, ActiveTime = time };
    }
}
