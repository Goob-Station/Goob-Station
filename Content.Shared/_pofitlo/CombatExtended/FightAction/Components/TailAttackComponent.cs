using Content.Shared._pofitlo.CombatExtended.FightAction;
using Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._pofitlo.CombatExtended.FightAction.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class TailAttackComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<CombatAnimationPrototype> TailAnimationPrototype = "TailAttackAnimation";

    [DataField, AutoNetworkedField]
    public ProtoId<CombatAnimationPrototype> MissAnimationPrototype = "PunchAnimation";

    [DataField, AutoNetworkedField]
    public Dictionary<string, object> AnimationSettings = new()
    {
        { "tailSwingAngle", 90.0f },
        { "tailSwingSpeed", 1.2f },
        { "tailFadeoutDelay", 0.1f }
    };
}
