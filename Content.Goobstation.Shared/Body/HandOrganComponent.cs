using Robust.Shared.Prototypes;

namespace Content.Shared.Body;

public sealed partial class HandOrganComponent : Component
{
    /// <summary>
    /// If not null, the hand will contain this item when it's attached to someone. You probably want to make sure the item is unremovable.
    /// </summary>
    [DataField]
    public EntProtoId? StartingItem;
}
