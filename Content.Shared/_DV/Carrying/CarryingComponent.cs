using Robust.Shared.GameStates;

namespace Content.Shared._DV.Carrying;

/// <summary>
/// Added to an entity when they are carrying somebody.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(CarryingSystem))]
[AutoGenerateComponentState]
public sealed partial class CarryingComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Carried;
}
