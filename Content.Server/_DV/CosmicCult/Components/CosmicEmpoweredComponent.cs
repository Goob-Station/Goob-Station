namespace Content.Server._DV.CosmicCult.Components;

/// <summary>
///     Component used for storing and handling the empowered cultists' speed boost.
/// </summary>
[RegisterComponent]
public sealed partial class CosmicEmpoweredSpeedComponent : Component
{
    public float SpeedBoost = 1.15f;
}
