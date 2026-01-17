using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Cult.Events;

public sealed partial class EventActionCultPrepareBloodMagic : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class EventActionCultPrepareBloodMagicDoAfter : SimpleDoAfterEvent
{
    public EntProtoId? SpellId;
}

public sealed partial class EventActionCultEmp : InstantActionEvent;

public sealed partial class EventActionCultDagger : InstantActionEvent;
