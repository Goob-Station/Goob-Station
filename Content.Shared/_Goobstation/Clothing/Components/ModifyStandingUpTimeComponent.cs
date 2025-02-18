using Content.Shared.Inventory;

namespace Content.Shared._Goobstation.Clothing.Components;

[RegisterComponent]
public sealed partial class ModifyStandingUpTimeComponent : Component
{
    [DataField]
    public float Multiplier = 1f;
}

public sealed class GetStandingUpTimeMultiplierEvent : EntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.FEET;

    public float Multiplier = 1f;
}
