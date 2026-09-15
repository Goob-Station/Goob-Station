using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Voidwalker;

[ByRefEvent]
public record struct VoidwalkerSpacedStatusChangedEvent(bool Spaced);

public sealed partial class VoidwalkerUnsettleEvent : EntityTargetActionEvent;

public sealed partial class VoidWhisperEvent : EntityTargetActionEvent;

public sealed partial class ExitNebulaCrawlEvent : InstantActionEvent;

public sealed partial class VoidwalkerVoidWalkEvent : WorldTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class VoidwalkerUnsettleDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class VoidwalkerKidnapDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class VoidwalkerConvertWallDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class CosmicSkullDoAfterEvent : SimpleDoAfterEvent;
