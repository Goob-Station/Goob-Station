using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.SmartLinkImplant;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SmartLinkComponent : Component
{
    [DataField, AutoNetworkedField]
    public float SpeedMultiplier = 0.5f;
}
