using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Phones.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RotaryPhoneHolderComponent : Component
{
    [DataField, AutoNetworkedField]
    public int? PhoneNumber;
}
