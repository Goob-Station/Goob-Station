namespace Content.Shared._Lavaland.Anger.Components;

/// <summary>
/// Scales HP or Anger amount depending on the amount of aggressors this entity has.
/// </summary>
[RegisterComponent]
public sealed partial class AngerPlayerScalingComponent : Component
{
    [DataField]
    public float? AngerScalingFactor;

    [DataField]
    public float? HealthScalingFactor;
}
