using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MartialArts.Events;
[Serializable, NetSerializable, DataDefinition]
public sealed partial class JudoThrowPerformedEvent : EntityEventArgs;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class JudoEyePokePerformedEvent : EntityEventArgs;


[Serializable, NetSerializable, DataDefinition]
public sealed partial class JudoArmbarPerformedEvent : EntityEventArgs;


[Serializable, NetSerializable, DataDefinition]
public sealed partial class JudoGoldenBlastPerformedEvent : EntityEventArgs;
