using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Phones.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RotaryPhoneHolderComponent : Component
{
    [DataField, AutoNetworkedField]
    public int? PhoneNumber;

    [DataField, AutoNetworkedField]
    public EntityUid? ConnectedPhone;

    [AutoNetworkedField]
    public SpriteSpecifier RopeSprite = new SpriteSpecifier.Rsi(new ResPath("_RMC14/Objects/phone/phone.rsi"), "rope");
}
