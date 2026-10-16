using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Mech.Components;

/// <summary>
/// Lets the mech move in 0 grav while installed.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MechThrusterModuleComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public bool AddedAlwaysTouching;

    [ViewVariables, AutoNetworkedField]
    public bool Installed;
}
