using Robust.Shared.GameStates;

namespace Content.Trauma.Shared.ShadowDemon;

/// <summary>
/// Modifies the movement speed on an entity based on its light levels
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class LightSpeedModifierComponent : Component
{
    /// <summary>
    /// The required light level for movement modifier to take place
    /// </summary>
    [DataField]
    public float RequiredLightLevel = 0.25f;

    [DataField]
    public float WalkModifier = 1f;

    [DataField]
    public float SprintModifier = 1f;

    /// <summary>
    /// If we are on light, based on the RequiredLightLevel
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool OnLight;
}
