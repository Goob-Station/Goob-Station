using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.MapRestricted.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MapRestrictedComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityWhitelist? MapWhitelist;

    [DataField, AutoNetworkedField]
    public EntityWhitelist? MapBlacklist;
}
