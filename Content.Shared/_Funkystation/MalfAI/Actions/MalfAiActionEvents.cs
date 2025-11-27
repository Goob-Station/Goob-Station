// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;

namespace Content.Shared._Funkystation.MalfAI.Actions;

/// <summary>
/// Action event for activating the Malf AI doomsday protocol.
/// </summary>
public sealed partial class MalfAiDoomsdayActionEvent : InstantActionEvent;

/// <summary>
/// Action event for the Malf AI Hijack Mech ability.
/// </summary>
public sealed partial class MalfAiHijackMechActionEvent : EntityTargetActionEvent;

/// <summary>
/// Action event for the Malf AI Shunt to APC ability.
/// </summary>
public sealed partial class MalfAiShuntToApcActionEvent : EntityTargetActionEvent;

/// <summary>
/// Action event for the Malf AI Set Viewport ability.
/// </summary>
public sealed partial class MalfAiSetViewportActionEvent : WorldTargetActionEvent;

/// <summary>
/// Action event for opening the Malf AI borgs UI.
/// </summary>
public sealed partial class OpenMalfAiBorgsUiActionEvent : InstantActionEvent;

/// <summary>
/// Action event for the Malf AI Detonate RCDs ability.
/// </summary>
public sealed partial class MalfAiDetonateRcdsActionEvent : InstantActionEvent;

/// <summary>
/// Action event for the Malf AI Lockdown Grid ability.
/// </summary>
public sealed partial class MalfAiLockdownGridActionEvent : InstantActionEvent
{
    /// <summary>
    /// Duration of the lockdown in seconds.
    /// </summary>
    public float Duration = 30f;
}

/// <summary>
/// Action event for the Malf AI Open Viewport ability.
/// </summary>
public sealed partial class MalfAiOpenViewportActionEvent : InstantActionEvent;

/// <summary>
/// Action event for the Malf AI Return to Core ability.
/// </summary>
public sealed partial class MalfAiReturnToCoreActionEvent : InstantActionEvent;

/// <summary>
/// Action event for the Malf AI Overload Machine ability.
/// </summary>
public sealed partial class MalfAiOverloadMachineActionEvent : WorldTargetActionEvent;

/// <summary>
/// Action event for the Malf AI Override Machine ability.
/// </summary>
public sealed partial class MalfAiOverrideMachineActionEvent : WorldTargetActionEvent;
