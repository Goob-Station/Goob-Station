using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Gangwars.Components;

/// <summary>
/// Added to the gang locker entity spawned by a gang leader.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class GangLockerComponent : Component
{
    [DataField]
    public ProtoId<FactionIconPrototype> StatusIcon = "GangLocker";

    [DataField, AutoNetworkedField]
    public EntityUid? OwnerLeader;

    [DataField, AutoNetworkedField]
    public Color? GangColor;

    [DataField, AutoNetworkedField]
    public EntityUid? CurrentStoreUser;
}
