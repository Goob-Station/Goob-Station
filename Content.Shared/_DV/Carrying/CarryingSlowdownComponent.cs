using Robust.Shared.GameStates;

namespace Content.Shared._DV.Carrying;

[RegisterComponent, NetworkedComponent, Access(typeof(CarryingSlowdownSystem))]
[AutoGenerateComponentState]
public sealed partial class CarryingSlowdownComponent : Component
{
    /// <summary>
    /// Modifier for both walk and sprint speed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Modifier = 1.0f;
}
