using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Revenant;

/// <summary>
/// Marks the user as using the ability
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ActiveTouchOfEvilComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public float ThrowSpeed;
}
