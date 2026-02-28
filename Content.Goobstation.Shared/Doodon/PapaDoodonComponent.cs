using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Shared.Doodons;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class PapaDoodonComponent : Component
{
    [DataField] public EntProtoId OrderStayAction = "ActionPapaDoodonOrderStay";
    [DataField] public EntityUid? OrderStayActionEntity;

    [DataField] public EntProtoId OrderFollowAction = "ActionPapaDoodonOrderFollow";
    [DataField] public EntityUid? OrderFollowActionEntity;

    [DataField] public EntProtoId OrderAttackAction = "ActionPapaDoodonOrderAttack";
    [DataField] public EntityUid? OrderAttackActionEntity;

    [DataField] public EntProtoId OrderLooseAction = "ActionPapaDoodonOrderLoose";
    [DataField] public EntityUid? OrderLooseActionEntity;

    [DataField("commandActionEntity")]
    public EntityUid? CommandActionEntity;

    [AutoNetworkedField]
    [DataField("currentOrder")]
    public DoodonOrderType CurrentOrder = DoodonOrderType.Follow;

    // Actions
    [DataField] public EntProtoId EstablishHallAction = "ActionDoodonEstablishTownHall";
    [DataField] public EntityUid? EstablishHallActionEntity;

    // Town hall placement
    [DataField] public EntProtoId TownHallPrototype = "DoodonTownHall";
    [DataField] public bool HallPlaced;
}

[Serializable, NetSerializable]
public enum DoodonOrderType : byte
{
    Stay,
    Follow,
    AttackTarget, // like CheeseEm (attack a chosen target)
    Loose         // free-roam aggro (SimpleHostile)
}
