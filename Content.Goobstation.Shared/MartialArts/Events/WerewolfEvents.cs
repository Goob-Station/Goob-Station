using Content.Shared.Damage;
using Robust.Shared.Audio;
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
public sealed partial class ViciousTossPerformedEvent : BaseWerewolfMoveEvent;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class DismembermentPerformedEvent: BaseWerewolfMoveEvent;
