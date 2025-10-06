using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Aura;

/// <summary>
/// Those who know 🥭🥭🥭
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class AuraComponent : Component
{
    /// <summary>
    /// How much aura u have type shiii ☠️☠️
    /// </summary>
    [DataField, AutoNetworkedField]
    public float AuraFarm = 0.5f;

    /// <summary>
    /// Oi oi... baka 😡😡
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color AuraColor = Color.Black;

    [DataField, AutoNetworkedField]
    public float Distortion = 0.05f;
}
