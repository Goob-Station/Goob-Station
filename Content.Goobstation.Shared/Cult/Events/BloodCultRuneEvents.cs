using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Cult.Events;

[ImplicitDataDefinitionForInheritors]
public abstract partial class CultRuneEvent : CancellableEntityEventArgs
{
    public bool Handled = false;

    [NonSerialized] public List<EntityUid>? Invokers;
    [NonSerialized] public List<EntityUid>? Targets;

    [DataField] public int RequiredInvokers = 1;

    [DataField] public Color PulseColor = Color.Black;

    [DataField] public LocId InvokeLoc = string.Empty;

    [DataField] public LocId InspectNameLoc;

    [DataField] public LocId InspectDescLoc;
}

public sealed partial class BloodCultRuneScribeSelectRuneMessage(EntProtoId id) : BoundUserInterfaceMessage
{
    public EntProtoId ID = id;
}

public sealed partial class CultRuneBasicEvent : CultRuneEvent;
public sealed partial class CultRuneOfferEvent : CultRuneEvent;
public sealed partial class CultRuneSacrificeEvent : CultRuneEvent;
