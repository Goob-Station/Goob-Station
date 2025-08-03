using Content.Shared.Whitelist;

namespace Content.Shared._White.Inventory.Components;

public abstract partial class BaseEquipOnComponent : Component
{
    [DataField(required: true)]
    public string Slot = "mask";

    [DataField]
    public string BlockingSlot = "head";

    [DataField]
    public float EquipProb = 1f;

    [DataField]
    public EntityWhitelist? Blacklist;

    [DataField]
    public bool Force = true;

}
