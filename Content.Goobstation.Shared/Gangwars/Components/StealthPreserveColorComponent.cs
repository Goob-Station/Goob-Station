using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Gangwars.Components;

/// <summary>
/// Hides an entity like the StealthComponent, but instead of using the cloaking shader it fades
/// the sprite's alpha. This keeps the sprite's RGB color intact (e.g. a gang color applied by another
/// system) while still making the entity invisible. the cloaking shader in stealth would otherwise
/// overwrite that color. (Trust me I tried to get it to work.. A lot)
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class StealthPreserveColorComponent : Component
{
    /// <summary>
    /// How visible the entity is: 1 is fully visible, 0 is fully invisible.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Visibility = 0f;
}
