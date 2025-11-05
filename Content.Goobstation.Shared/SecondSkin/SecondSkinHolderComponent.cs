using Content.Shared.Containers.ItemSlots;
using Content.Shared.Inventory;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.SecondSkin;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SecondSkinHolderComponent : Component
{
    [ViewVariables]
    public ContainerSlot Container = default!;

    [DataField(required: true)]
    public ItemSlot ItemSlot;

    [DataField]
    public string ContainerId = "skin_slot";

    [DataField]
    public string Slot = "outerClothing";

    [DataField]
    public SlotFlags Flags = SlotFlags.OUTERCLOTHING;

    [DataField]
    public string State = "equipped-OUTERCLOTHING";

    [DataField]
    public EntProtoId SecondSkinActionId = "ActionActivateSecondSkin";

    [DataField, AutoNetworkedField]
    public EntityUid? SecondSkinAction;

    [DataField]
    public SpriteSpecifier.Rsi Sprite = new(new ResPath("_Goobstation/Clothing/OuterClothing/second_skin.rsi"), "icon");
}
