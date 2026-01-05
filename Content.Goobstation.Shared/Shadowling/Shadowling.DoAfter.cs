using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Shadowling;

/// <summary>
/// This handles Sling's DoAfter Events
/// </summary>
[Serializable, NetSerializable]
public sealed partial class EnthrallDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class RapidRehatchDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class NullChargeDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class BlackRecuperationDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class LightningStormEventDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class EmpoweredEnthrallDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class NoxImperiiDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class AscendanceDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class AntiMindControlItemDoAfterEvent : SimpleDoAfterEvent;
