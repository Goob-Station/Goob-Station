using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Cult.Events;

public abstract class CultRuneEvent : CancellableEntityEventArgs
{
    [NonSerialized] public List<EntityUid>? Invokers;
    [NonSerialized] public List<EntityUid>? Targets;

    public int RequiredInvokers = 1;

    public Color PulseColor = Color.Black;

    /// <summary>
    ///     What will people say when the rune is activated.
    /// </summary>
    [DataField] public LocId InvokeLoc = string.Empty;

    [DataField] public LocId InspectNameLoc;

    [DataField] public LocId InspectDescLoc;
}

public sealed partial class BloodCultRuneScribeSelectRuneMessage(EntProtoId id) : BoundUserInterfaceMessage
{
    public EntProtoId ID = id;
}

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
