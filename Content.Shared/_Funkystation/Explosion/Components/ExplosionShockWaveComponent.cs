using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.Explosion.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ExplosionShockWaveComponent : Component
{
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float FalloffPower = 40f;

    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float Sharpness = 10.0f;

    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float Width = 0.8f;
}
