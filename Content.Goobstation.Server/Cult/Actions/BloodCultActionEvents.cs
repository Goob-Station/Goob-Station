using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Cult.Actions;

public sealed partial class EventActionCultPrepareBloodMagic : InstantActionEvent;
public sealed partial class EventActionCultPrepareBloodMagicDoAfter : DoAfterEvent
{
    public EntProtoId? SpellId;

    public override DoAfterEvent Clone()
    {
        return new EventActionCultPrepareBloodMagicDoAfter()
        {
            SpellId = this.SpellId
        };
    }
}

public sealed partial class EventActionCultEmp : InstantActionEvent;

public sealed partial class EventActionCultDagger : InstantActionEvent;
