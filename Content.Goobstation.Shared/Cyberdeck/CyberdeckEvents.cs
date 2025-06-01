using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Cyberdeck;

public sealed partial class CyberdeckHackActionEvent : EntityTargetActionEvent;

/// <summary>
/// Puts the user in a projection entity that has AI view everywhere.
/// </summary>
public sealed partial class CyberdeckVisionEvent : InstantActionEvent;

/// <summary>
/// Returns user from his projection to his original entity.
/// </summary>
public sealed partial class CyberdeckVisionReturnEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class CyberdeckHackDoAfterEvent : SimpleDoAfterEvent;
