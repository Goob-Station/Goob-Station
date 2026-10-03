using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Vision;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RestrictFovComponent : Component
{
    [DataField, AutoNetworkedField]
    public float FovDegrees = 90f;

    [DataField]
    public float FeatherDegrees = 14f;

    [DataField]
    public float OutsideFovOpacity = 1f;

    /// <summary>
    /// The circle near the user.
    /// </summary>
    [DataField]
    public float NearUserPityRadius = 1.2f;

    [DataField]
    public float NearFeather = 0.4f;
}
