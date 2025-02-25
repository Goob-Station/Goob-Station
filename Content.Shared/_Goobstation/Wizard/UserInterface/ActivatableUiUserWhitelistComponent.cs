using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.UserInterface;

[RegisterComponent, NetworkedComponent]
public sealed partial class ActivatableUiUserWhitelistComponent : Component
{
    [DataField(required: true)]
    public EntityWhitelist Whitelist;
}
