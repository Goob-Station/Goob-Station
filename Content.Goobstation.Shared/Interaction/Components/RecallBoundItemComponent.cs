using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Interaction.Components;

[RegisterComponent]
public sealed partial class RecallBoundItemComponent : Component
{
    [DataField]
    // Item UID → Action UID
    public Dictionary<EntityUid, EntityUid> BoundItems = new();

    [DataField]
    public EntProtoId RecallAction = "ActionRecallBoundItem";

    public EntityUid? RecallActionEntity;
}
