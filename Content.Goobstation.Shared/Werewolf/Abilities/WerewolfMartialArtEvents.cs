using Content.Shared.Damage;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Werewolf.Abilities;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class LyCqcOpenVeinPerformedEvent : EntityEventArgs;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class LyCqcViciousTossEvent : EntityEventArgs
{
    [DataField]
    public DamageSpecifier DamageThrow;

    [DataField]
    public float ThrowSpeed = 20;
};

[Serializable, NetSerializable, DataDefinition]
public sealed partial class LyCqcDismembermentPerformedEvent : EntityEventArgs
{
    [DataField]
    public ProtoId<TagPrototype> Head = "Head";
};
