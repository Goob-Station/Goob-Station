using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wizard.UserInterface;

[RegisterComponent, NetworkedComponent]
public sealed partial class ActivatableUiUserWhitelistComponent : Component
{
    [DataField(required: true)]
    public EntityWhitelist Whitelist;
}
