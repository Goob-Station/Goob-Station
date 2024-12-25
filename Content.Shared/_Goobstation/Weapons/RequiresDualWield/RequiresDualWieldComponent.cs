using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Weapons.RequiresDualWield;

/// <summary>
/// Makes a weapon only able to be shot while dual wielding.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(RequiresDualWieldSystem))]
[AutoGenerateComponentState]
public sealed partial class RequiresDualWieldComponent : Component
{

    [DataField, AutoNetworkedField]
    public TimeSpan LastPopup;

    [DataField, AutoNetworkedField]
    public TimeSpan PopupCooldown = TimeSpan.FromSeconds(1);

    [DataField]
    public LocId? WieldRequiresExamineMessage  = "gun-requires-dual-wield-component-examine";

}
