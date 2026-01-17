using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Cult.Events;

[ImplicitDataDefinitionForInheritors, Serializable, NetSerializable]
public abstract partial class CultRuneEvent
{
    public bool Cancelled = false;
    public bool Handled = false;

    [NonSerialized] public List<EntityUid>? Invokers;
    [NonSerialized] public List<EntityUid>? Targets;

    [DataField] public int RequiredInvokers = 1;

    [DataField] public Color PulseColor = Color.Black;

    [DataField] public LocId InvokeLoc = string.Empty;

    [DataField] public LocId InspectNameLoc;

    [DataField] public LocId InspectDescLoc;

    [DataField] public DamageSpecifier? Damage;
}

[Serializable, NetSerializable]
public sealed partial class BloodCultRuneScribeSelectRuneMessage(EntProtoId id) : BoundUserInterfaceMessage
{
    public EntProtoId Rune = id;
}

[Serializable, NetSerializable]
public sealed partial class RuneScribeDoAfter : DoAfterEvent
{
    [DataField] public EntProtoId Rune;

    public RuneScribeDoAfter(EntProtoId rune)
    {
        Rune = rune;
    }

    public override DoAfterEvent Clone() => this;
}

[Serializable, NetSerializable]
public sealed partial class RuneScribeRemoveDoAfter : DoAfterEvent
{
    [NonSerialized] public EntityUid Rune;

    public RuneScribeRemoveDoAfter(EntityUid rune)
    {
        Rune = rune;
    }

    public override DoAfterEvent Clone() => this;
}

public sealed partial class CultRuneBasicEvent : CultRuneEvent;
public sealed partial class CultRuneMalfEvent : CultRuneEvent;
public sealed partial class CultRuneOfferEvent : CultRuneEvent;
public sealed partial class CultRuneSacrificeEvent : CultRuneEvent;
public sealed partial class CultRuneEmpowerEvent : CultRuneEvent;
public sealed partial class CultRuneTeleportEvent : CultRuneEvent;
