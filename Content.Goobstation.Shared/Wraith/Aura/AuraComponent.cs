using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Aura;

/// <summary>
/// Creates an aura around you.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class AuraComponent : Component
{
    /// <summary>
    /// The intensity of the aura
    /// </summary>
    [DataField, AutoNetworkedField]
    public float AuraFarm = 0.5f;

    [DataField, AutoNetworkedField]
    public Color AuraColor = Color.Black;

    /// <summary>
    /// How much to distort the aura
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Distortion = 0.05f;

    /// <summary>
    /// If not null, it will use multi shader overlay to render with specified render order
    /// </summary>
    [DataField, AutoNetworkedField]
    public int? MultiShaderOrder = 5;
}
