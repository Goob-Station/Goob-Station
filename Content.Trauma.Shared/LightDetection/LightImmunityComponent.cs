using Robust.Shared.GameStates;

namespace Content.Trauma.Shared.LightDetection;

/// <summary>
/// Grants light immunity for X seconds
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class LightImmunityComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(10);

    [ViewVariables, AutoNetworkedField]
    public TimeSpan NextUpdate;
}
