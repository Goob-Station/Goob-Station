using Content.Shared.Damage;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MartialArts.Events;

[Serializable, NetSerializable, ImplicitDataDefinitionForInheritors]
public abstract partial class BaseWerewolfMoveEvent : EntityEventArgs
{
    [DataField]
    public virtual SoundSpecifier Sound { get; set; } = new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg");
}


[Serializable, NetSerializable, DataDefinition]
public sealed partial class OpenVeinPerformedEvent : BaseWerewolfMoveEvent
{
    [DataField]
    public float BleedAmount;
};

[Serializable, NetSerializable, DataDefinition]
public sealed partial class ViciousTossPerformedEvent : BaseWerewolfMoveEvent
{
    [DataField]
    public DamageSpecifier DamageThrow;

    [DataField]
    public float ThrowSpeed = 20;
};

[Serializable, NetSerializable, DataDefinition]
public sealed partial class DismembermentPerformedEvent : BaseWerewolfMoveEvent
{
    [DataField]
    public ProtoId<TagPrototype> Head = "Head";
};
