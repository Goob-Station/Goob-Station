using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Cult.Events;

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
