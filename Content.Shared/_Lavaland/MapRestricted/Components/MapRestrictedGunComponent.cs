using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.MapRestricted.Components;

/// <summary>
/// Makes a gun only usable on a planet that passes a whitelist.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MapRestrictedGunComponent : Component
{
    [DataField]
    public LocId? PopupOnBlock;
}
