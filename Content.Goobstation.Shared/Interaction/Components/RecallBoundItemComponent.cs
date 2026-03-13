using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Interaction.Components;

[RegisterComponent]
public sealed partial class RecallBoundItemComponent : Component
{
    /// <summary>
    /// Item UID + Action UID
    /// </summary>
    [DataField]
    public Dictionary<EntityUid, EntityUid> BoundItems = new();

    /// <summary>
    /// Gets or sets the prototype identifier for the recall action associated with a bound item.
    /// </summary>
    [DataField]
    public EntProtoId RecallAction = "ActionRecallBoundItem";

    public EntityUid? RecallActionEntity;
}
