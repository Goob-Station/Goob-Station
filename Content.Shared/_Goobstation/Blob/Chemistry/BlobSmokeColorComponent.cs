using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Blob.Chemistry;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class BlobSmokeColorComponent : Component
{
    [AutoNetworkedField, ViewVariables]
    public Color Color { get; set; } = Color.White;
}
