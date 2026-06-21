using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;

[DataDefinition]
[Prototype("fightAction")]
public sealed partial class FightActionPrototype : IPrototype
{
    /// <summary>
    ///     Unique identifier of the prototype.
    /// </summary>
    [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    ///     Localization id of the action's display name. Used for codex and radial menu.
    /// </summary>
    [DataField] public string LocName = "fight-action-unknown";

    /// <summary>
    ///     Localization id of the action's description. Used for codex.
    /// </summary>
    [DataField] public string LocDesc = string.Empty;

    /// <summary>
    ///     Icon for codex and radial menu.
    /// </summary>
    [DataField] public SpriteSpecifier Icon = new SpriteSpecifier.Rsi(new("_pofitlo/CombatExtended/FightAction/punch.rsi"), "icon");

    /// <summary>
    ///     Which attack strategy (combat behaviour) this action triggers, e.g. punch or tail attack.
    /// </summary>
    [DataField] public AttackStrategy SetAttackStrategy = AttackStrategy.Punch;

    /// <summary>
    ///     Animation played on the main (primary) attack.
    /// </summary>
    [DataField] public ProtoId<CombatAnimationPrototype> AnimationPrototype = "PunchAnimation";

    /// <summary>
    ///     Animation played on the alternative (secondary) attack. Null falls back to no special animation.
    /// </summary>
    [DataField] public ProtoId<CombatAnimationPrototype>? AltAnimationPrototype;

    /// <summary>
    ///     If true, this fight action takes precedence over a held weapon when resolving which attack to use.
    /// </summary>
    [DataField] public bool HasHigherPriorityThanWeapons = false;
}
