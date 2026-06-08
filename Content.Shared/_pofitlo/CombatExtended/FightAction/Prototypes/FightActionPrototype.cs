using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;

[Serializable, NetSerializable, DataDefinition]
[Prototype("fightAction")]
public sealed partial class FightActionPrototype : IPrototype, ICloneable
{
    [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    ///     Used for codex and radial menu.
    /// </summary>
    [DataField] public string LocName = "fight-action-unknown";

    /// <summary>
    ///     Used for codex
    /// </summary>
    [DataField] public string LocDesc = string.Empty;

    /// <summary>
    ///     Icon for codex and radial menu.
    /// </summary>
    [DataField] public SpriteSpecifier Icon = new SpriteSpecifier.Rsi(new("_Goobstation/Heretic/amber_focus.rsi"), "icon"); // TODO поставить свою

    //TODO дать описание
    [DataField] public AttackStrategy SetAttackStrategy = AttackStrategy.Punch;

    [DataField] public ProtoId<CombatAnimationPrototype> AnimationPrototype = "PunchAnimation";

    [DataField] public ProtoId<CombatAnimationPrototype> AltAnimationPrototype = "PunchAnimation"; //TODO Это заглушка. Нуэно сделать так, что бы оно не было обязательным

    [DataField] public ProtoId<FightActionMeleeParametersPrototype> MeleeParametersPrototype = "PunchMeleeParameters";

    [DataField] public bool HasHigherPriorityThanWeapons = false;

    public object Clone()
    {
        return new FightActionPrototype() // TODO сдлать что-то с тем, что оно ругается
        {
            ID = ID,
            LocName = LocName,
            LocDesc = LocDesc,
            Icon = Icon,
            SetAttackStrategy = SetAttackStrategy,
            AnimationPrototype = AnimationPrototype,
            AltAnimationPrototype = AltAnimationPrototype,
            MeleeParametersPrototype = MeleeParametersPrototype,
            HasHigherPriorityThanWeapons = HasHigherPriorityThanWeapons
        };
    }
}
