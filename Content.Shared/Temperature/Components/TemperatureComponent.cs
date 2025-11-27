using Content.Shared.Atmos;

namespace Content.Shared.Temperature.Components;

/// <summary>
/// Added here by goobstation due to other systems moving to share and needing to check if this exists. - See the serverside temperaturecomp
///
/// Handles changing temperature,
/// informing others of the current temperature,
/// and taking fire damage from high temperature.
/// </summary>
public abstract class SharedTemperatureComponent : Component
{
    /// <summary>
    /// Surface temperature which is modified by the environment.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float CurrentTemperature = Atmospherics.T20C;
}
