using Content.Shared.Inventory;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.Ranching.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ReplaceOnItemEquippedComponent : Component
{
    [DataField]
    public EntProtoId Ent;

    [DataField]
    public HashSet<ProtoId<TagPrototype>> RequiredTags = new();

    [DataField]
    public SlotFlags Slots = SlotFlags.MASK;
}
