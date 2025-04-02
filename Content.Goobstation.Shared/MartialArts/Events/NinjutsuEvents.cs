using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MartialArts.Events;

[Serializable, NetSerializable, ImplicitDataDefinitionForInheritors]
public abstract partial class BaseNinjutsuEvent : EntityEventArgs
{
    [DataField]
    public virtual SoundSpecifier Sound { get; set; } = new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg");
}

[DataDefinition]
public sealed partial class NinjutsuTakedownPerformedEvent : BaseNinjutsuEvent
{
    [DataField]
    public float BackstabMultiplier = 2.5f;
}

[DataDefinition]
public sealed partial class BiteTheDustPerformedEvent : BaseNinjutsuEvent;

[DataDefinition]
public sealed partial class DirtyKillPerformedEvent : BaseNinjutsuEvent;
