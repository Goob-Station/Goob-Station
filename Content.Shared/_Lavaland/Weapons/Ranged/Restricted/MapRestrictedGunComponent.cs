using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Weapons.Ranged.Restricted;

/// <summary>
/// Makes a gun only usable on a planet that passes a whitelist.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MapRestrictedGunComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityWhitelist? PlanetWhitelist;

    [DataField, AutoNetworkedField]
    public EntityWhitelist? PlanetBlacklist;

    [DataField, AutoNetworkedField]
    public LocId? PopupOnBlock;
}
