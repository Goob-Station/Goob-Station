using Content.Shared.Inventory;

namespace Content.Shared.Temperature;

public sealed class ModifyChangedTemperatureEvent : EntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; } = ~SlotFlags.POCKET;

    public float TemperatureDelta;
    public readonly EntityUid Target; // Goobstation

    public ModifyChangedTemperatureEvent(float temperature, EntityUid target) // Goobstation
    {
        TemperatureDelta = temperature;
        Target = target; // Goobstation
    }
}

public sealed class OnTemperatureChangeEvent : EntityEventArgs
{
    public readonly float CurrentTemperature;
    public readonly float LastTemperature;
    public readonly float TemperatureDelta;

    public OnTemperatureChangeEvent(float current, float last, float delta)
    {
        CurrentTemperature = current;
        LastTemperature = last;
        TemperatureDelta = delta;
    }
}

