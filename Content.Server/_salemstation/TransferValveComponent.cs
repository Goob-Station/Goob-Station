using Content.Shared.Containers.ItemSlots;

namespace Content.Server.SalemStation;

[RegisterComponent]
public sealed partial class TransferValveComponent : Component
{
    public const string PlasmaTankSlotId = "plasma_tank";
    public const string OxygenTankSlotId = "oxygen_tank";
    public const string TimerSlotId = "timer";

    [DataField]
    public ItemSlot PlasmaTankSlot = new();

    [DataField]
    public ItemSlot OxygenTankSlot = new();

    [DataField]
    public ItemSlot TimerSlot = new();

    public bool TimerWasActive;
    public bool Detonated;
}
