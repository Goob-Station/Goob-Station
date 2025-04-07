using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Blob.Chemistry;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class BlobSmokeColorComponent : Component
{
    [AutoNetworkedField, ViewVariables]
    public Color Color { get; set; } = Color.White;
}
