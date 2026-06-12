using Robust.Shared.GameStates;
using Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Content.Shared.Damage;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._pofitlo.CombatExtended.FightAction;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class FightActionComponent : Component
{

    [DataField, AutoNetworkedField]
    public List<ProtoId<FightActionPrototype>> AvailableActions = new();

    [DataField, AutoNetworkedField]
    public AttackStrategy Strategy;

    [DataField, AutoNetworkedField]
    public EntProtoId Animation = "FightActionArcMainTailAttack";

    [DataField, AutoNetworkedField]
    public EntProtoId AltAnimation = "FightActionArcAltTailAttack";

    [DataField, AutoNetworkedField]
    public ProtoId<CombatAnimationPrototype>? CombatAnimationPrototype = "PunchAnimation";

    [DataField, AutoNetworkedField]
    public ProtoId<CombatAnimationPrototype>? AltCombatAnimationPrototype = "PunchAnimation";

    [DataField, AutoNetworkedField]
    public ProtoId<FightActionMeleeParametersPrototype>? FightActionMeleeParametersPrototype = "PunchMeleeParameters";

    [DataField, AutoNetworkedField]
    public Dictionary<string, object> AnimationSettings = new();

    [DataField, AutoNetworkedField]
    public bool HasHigherPriorityThanWeapons = false;

}
