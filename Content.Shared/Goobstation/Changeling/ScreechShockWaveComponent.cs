using Robust.Shared.GameStates;

namespace Content.Shared.Changeling;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ScreechShockWaveComponent : Component
{
    /// <summary>
    ///     The rate at which the wave fades, lower values means it's active for longer.
    /// </summary>
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float FalloffPower = 40f;

    /// <summary>
    ///     How sharp the wave distortion is. Higher values make the wave more pronounced.
    /// </summary>
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float Sharpness = 5.0f;

    /// <summary>
    ///     Width of the wave.
    /// </summary>
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float Width = 0f;
}
