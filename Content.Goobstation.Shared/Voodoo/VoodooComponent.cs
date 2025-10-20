using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Voodoo;

/// <summary>
/// Component Used to track people with a certain name for the voodoo system
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VoodooComponent : Component
{
    [DataField("targetName"), AutoNetworkedField]
    public string TargetName;
}
