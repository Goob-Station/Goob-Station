using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Actions;

/// <summary>
/// Checks whitelist/blacklist against the user. If they don't pass, the action cancels.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ActionUserWhitelistComponent : Component
{
    [DataField]
    public EntityWhitelist? Whitelist = new();

    [DataField]
    public LocId? Popup = "whitelist-action-generic-fail";
}
