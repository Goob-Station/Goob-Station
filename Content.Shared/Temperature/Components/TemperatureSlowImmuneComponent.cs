using Content.Shared.Temperature.Systems;

namespace Content.Shared.Temperature.Components;

/// <summary>
/// Makes the entity unaffected by low temperature slowdown || GOOB EDIT ||
/// </summary>
[RegisterComponent, Access(typeof(SharedTemperatureSystem))]
public sealed partial class TemperatureSlowImmuneComponent : Component
{
}
