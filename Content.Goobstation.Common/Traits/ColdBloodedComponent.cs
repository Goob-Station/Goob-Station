namespace Content.Goobstation.Common.Traits;

/// <summary>
/// Component indicating that a mob is cold-blooded. When this is present on a mob,
/// it will cause ThermalRegulatorSystem to ignore it and will tweak its temperature
/// damage thresholds by the values in ColdBloodedSystem
/// </summary>
[RegisterComponent]
public sealed partial class ColdBloodedComponent : Component;
