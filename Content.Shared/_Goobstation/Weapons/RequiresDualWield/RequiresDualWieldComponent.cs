using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.GameStates;
using Content.Shared.Whitelist;

namespace Content.Shared._Goobstation.Weapons.RequiresDualWield;

/// <summary>
/// Makes a weapon only able to be shot while dual wielding.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(RequiresDualWieldSystem))]
public sealed partial class RequiresDualWieldComponent : Component
{
    public TimeSpan LastPopup;

    [DataField]
    public TimeSpan PopupCooldown = TimeSpan.FromSeconds(1);

    [DataField]
    public LocId? WieldRequiresExamineMessage  = "gun-requires-dual-wield-component-examine";

    [DataField]
    public EntityWhitelist? Whitelist;
}
