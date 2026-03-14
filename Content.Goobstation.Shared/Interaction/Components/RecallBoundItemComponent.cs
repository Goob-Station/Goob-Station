using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Interaction.Components;

[RegisterComponent]
public sealed partial class RecallBoundItemComponent : Component
{
    /// <summary>
    /// The item bound to this user.
    /// </summary>
    [DataField]
    public EntityUid? BoundItem;

    /// <summary>
    /// Prototype of the recall action.
    /// </summary>
    [DataField]
    public EntProtoId RecallAction = "ActionRecallBoundItem";

    /// <summary>
    /// The spawned action entity.
    /// </summary>
    public EntityUid? RecallActionEntity;
}
