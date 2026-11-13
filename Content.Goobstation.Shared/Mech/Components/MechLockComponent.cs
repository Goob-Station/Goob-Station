using Content.Shared.Access;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Mech.Components;

/// <summary>
/// Allows a mech to be locked from the inside. Requires <see cref="RequiredAccess"/> to lock it.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MechLockComponent : Component
{
    [DataField]
    public EntProtoId ActionProto = "ActionMechToggleLock";

    [ViewVariables, AutoNetworkedField]
    public EntityUid? ActionEntity;

    [DataField, AutoNetworkedField]
    public bool RequireAccess = true;

    [DataField, AutoNetworkedField]
    public List<ProtoId<AccessLevelPrototype>> RequiredAccess = new() { "Security" };

    [ViewVariables, AutoNetworkedField]
    public EntityUid? PendingUser;

    [ViewVariables, AutoNetworkedField]
    public HashSet<ProtoId<AccessLevelPrototype>> LockedAccess = new();
}
