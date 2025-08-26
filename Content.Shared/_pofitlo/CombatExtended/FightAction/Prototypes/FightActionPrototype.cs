using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Content.Shared.Materials;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using Content.Shared._pofitlo.CombatExtended.FightAction;

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
    [DataField] public AttackStrategy СhosenAttackStrategy = AttackStrategy.Punch;

    public object Clone()
    {
        return new FightActionPrototype()
        {
            ID = ID,
            LocName = LocName,
            LocDesc = LocDesc,
            Icon = Icon,
            СhosenAttackStrategy = СhosenAttackStrategy
        };
    }
}
