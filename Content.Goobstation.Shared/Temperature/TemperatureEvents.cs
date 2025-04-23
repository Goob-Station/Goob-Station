using Content.Shared.Atmos;

namespace Content.Goobstation.Shared.Temperature;

public sealed class TemperatureImmunityEvent : EntityEventArgs
{
    public bool IsImmune = false; // completely immune to temperature?
    public bool HighImmune = false; // immune to HIGH temperature?
    public bool LowImmune = false; // immune to LOW temperature?
    public readonly float IdealTemperature = Atmospherics.T37C;
}
