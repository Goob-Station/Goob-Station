// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.MalfAI.Actions;

public sealed partial class MalfAiDoomsdayActionEvent : InstantActionEvent;

public sealed partial class OpenMalfAiBorgsUiActionEvent : InstantActionEvent;

public sealed partial class MalfAiDetonateRcdsActionEvent : InstantActionEvent;

public sealed partial class MalfAiLockdownGridActionEvent : InstantActionEvent
{
    [DataField]
    public float Duration = 30f;
}

public sealed partial class MalfAiOpenViewportActionEvent : InstantActionEvent;

public sealed partial class MalfAiReturnToCoreActionEvent : InstantActionEvent;

public sealed partial class MalfAiVoiceModulatorActionEvent : InstantActionEvent;

public sealed partial class MalfAiToggleCameraUpgradeActionEvent : InstantActionEvent;

public sealed partial class MalfAiToggleCameraMicrophonesActionEvent : InstantActionEvent;

public sealed partial class MalfAiRoboticsFactoryActionEvent : WorldTargetActionEvent;

public sealed partial class MalfAiGyroscopeActionEvent : WorldTargetActionEvent;

public sealed partial class MalfAiHijackMechActionEvent : EntityTargetActionEvent;

public sealed partial class MalfAiShuntToApcActionEvent : EntityTargetActionEvent;

public sealed partial class MalfAiSetViewportActionEvent : WorldTargetActionEvent;

public sealed partial class MalfAiOverloadMachineActionEvent : WorldTargetActionEvent;

public sealed partial class MalfAiOverrideMachineActionEvent : WorldTargetActionEvent;
