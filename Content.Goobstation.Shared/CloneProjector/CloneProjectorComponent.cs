using Content.Shared.Damage.Prototypes;
using Content.Shared.Inventory;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.CloneProjector;

[RegisterComponent]
public sealed partial class CloneProjectorComponent : Component
{
    /// <summary>
    /// The UID of the active clone.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? CloneUid;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? CurrentHost;

    /// <summary>
    /// Is the clone currently out?
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsActive;

    [DataField]
    public ComponentRegistry? AddedComponents;

    [DataField]
    public ComponentRegistry? RemovedComponents;

    [DataField]
    public ProtoId<DamageModifierSetPrototype> CloneDamageModifierSet ="LivingLight";

    [DataField]
    public ProtoId<InventoryTemplatePrototype> CloneInventoryTemplate = "holoclown";

    [ViewVariables(VVAccess.ReadOnly)]
    public Container CloneContainer = new();

    /// <summary>
    /// The ID of the action used to activate the projector.
    /// </summary>
    [DataField]
    public EntProtoId Action = "ActionActivateProjector";

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ActionEntity;
}
