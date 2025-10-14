using Robust.Shared.GameStates;

namespace Content.Shared._DV.Weather.Components;

/// <summary>
/// Makes an entity not take damage from any weather.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WeatherImmuneComponent : Component;
