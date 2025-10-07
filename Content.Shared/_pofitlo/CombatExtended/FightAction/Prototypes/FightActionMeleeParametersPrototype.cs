using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;

[Serializable, NetSerializable, DataDefinition]
[Prototype("fightActionMeleeParameters")]
public sealed partial class FightActionMeleeParametersPrototype : IPrototype, ICloneable
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField] public bool HasDisarm = false;

    public object Clone()
    {
        return new FightActionMeleeParametersPrototype()
        {
            ID = ID,
            HasDisarm = HasDisarm
        };
    }
}
