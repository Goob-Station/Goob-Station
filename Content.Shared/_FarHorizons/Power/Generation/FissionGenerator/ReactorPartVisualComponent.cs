using Robust.Shared.GameStates;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

/// <summary>
/// A component for the visual grid on top of the nuclear reactor
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ReactorPartVisualComponent : Component 
{
    [DataField, AutoNetworkedField]
    public Color color = Color.FromHex("#FF00FF");
}