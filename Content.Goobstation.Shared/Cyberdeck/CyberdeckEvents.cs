// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Cyberdeck;

/// <summary>
/// This handles finding the real target that the player want to hack,
/// and then starts a DoAfter if succeeded.
/// </summary>
public sealed partial class CyberdeckHackActionEvent : EntityTargetActionEvent;

/// <summary>
/// Puts the user in a projection entity that has AI view everywhere.
/// </summary>
public sealed partial class CyberdeckVisionEvent : InstantActionEvent;

/// <summary>
/// Returns user from his projection to his original entity.
/// </summary>
public sealed partial class CyberdeckVisionReturnEvent : InstantActionEvent;

/// <summary>
/// Raised on a CyberdeckHackable device, will take charges from the user and then
/// raise CyberdeckHackDeviceEvent to add actual effects on a target device.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class CyberdeckHackDoAfterEvent : SimpleDoAfterEvent;

/// <summary>
/// Shows some info to user that clicked the alert.
/// </summary>
public sealed partial class CyberdeckInfoAlertEvent : BaseAlertEvent;

/// <summary>
/// Actually hacks a device. Subscribe to it to handle effects that happen
/// after CyberdeckHackDoAfterEvent had run and after charges were taken from the user.
/// </summary>
/// <remarks>
/// Use Refund property VERY CAREFULLY. If true, it will return charges back to the user EVEN if other effects succeeded.
/// </remarks>
[ByRefEvent]
public record struct CyberdeckHackDeviceEvent(EntityUid User, bool Refund = false);

/// <summary>
/// Raised on an original target entity and code selected target
/// to get some information before starting the hacking do-after.
/// </summary>
/// <param name="OriginalTarget">Target originally selected by the player by doing an in-world action.</param>
/// <param name="Target">Target that the code logic selected to be hacked at the end. If not equal to OriginalTarget, then it is contained inside it.</param>
/// <param name="PenaltyTime">Timespan to add to the hacking do-after duration.</param>
/// <param name="Handled">If true, the penalty for hacking was already added to the player.</param>
[ByRefEvent]
public record struct BeforeCyberdeckHackEvent(EntityUid OriginalTarget, EntityUid Target, TimeSpan PenaltyTime, bool Handled);

/// <summary>
/// Raised on an original target and code selected target after the cyberdeck hacking do-after started successfully.
/// </summary>
/// <param name="OriginalTarget">Target originally selected by the player by doing an in-world action.</param>
/// <param name="Target">Target that the code logic selected to be hacked at the end. If not equal to OriginalTarget, then it is contained inside it.</param>
/// <param name="Handled">If true, the warning message about hacking was already shown to the player.</param>
[ByRefEvent]
public record struct AfterCyberdeckHackEvent(EntityUid OriginalTarget, EntityUid Target, bool Handled);
