using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Gangwars.Components;

/// <summary>
/// Added to a player's mob entity when selected as a Gang Leader.
/// Grants an action to summon or relocate their personal gang locker.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GangLeaderComponent : Component
{
    [DataField]
    public ProtoId<FactionIconPrototype> StatusIcon = "GangHead";

    [DataField]
    public EntProtoId SummonLockerAction = "ActionGangLeaderSummonLocker";

    [DataField]
    public EntityUid? ActionEnt;

    [DataField]
    public EntProtoId MemberOfferAction = "ActionGangLeaderMemberOffer";

    [DataField]
    public EntityUid? MemberOfferActionEnt;

    /// <summary>
    /// How many members the leader has recruited so far.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int InvitesUsed;

    /// <summary>
    /// Maximum number of members the leader is allowed to recruit.
    /// </summary>
    [DataField]
    public int MaxInvites = 3;

    [DataField, AutoNetworkedField]
    public EntityUid? PendingInviteTarget;

    [DataField]
    public float CrateExclusionZone = 10f;

    [DataField]
    public EntProtoId GangLockerPrototype = "GangLocker";

    [DataField, AutoNetworkedField]
    public EntityUid? GangLocker;

    [DataField]
    public Color? PendingRemakeColor;
}
