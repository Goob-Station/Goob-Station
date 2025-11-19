using Content.Goobstation.Shared.InternalResources.Data;

namespace Content.Goobstation.Shared.InternalResources.Events;

[ByRefEvent]
public sealed class InternalResourcesAmountChangeAttemptEvent(EntityUid uid, InternalResourcesData data, float amount) : CancellableEntityEventArgs
{
    public EntityUid EntityUid { get; } = uid;
    public InternalResourcesData InternalResources { get; } = data;
    public float ChangeAmount { get; } = amount;

}

public record struct InternalResourcesAmountChangedEvent(EntityUid Uid, InternalResourcesData Data, float PreviousAmount, float NewAmount, float Delta);
