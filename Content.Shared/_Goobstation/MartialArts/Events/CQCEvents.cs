using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.MartialArts.Events;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class CqcSlamPerformedEvent : EntityEventArgs;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class  CqcKickPerformedEvent : EntityEventArgs;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class CqcRestrainPerformedEvent : EntityEventArgs;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class CqcPressurePerformedEvent : EntityEventArgs;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class CqcConsecutivePerformedEvent : EntityEventArgs;
