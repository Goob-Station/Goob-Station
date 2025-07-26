// SPDX-FileCopyrightText: 2025 ThunderBear2006 <bearthunder06@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Silicon.MalfAI.Components;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Silicon.MalfAI.Events;

[Serializable, NetSerializable]
public sealed partial class HackDoAfterEvent : SimpleDoAfterEvent;

[ByRefEvent]
public readonly struct OnHackedEvent(Entity<MalfStationAIComponent> hacker)
{
    public readonly Entity<MalfStationAIComponent> HackerEntity = hacker;
};

public sealed partial class MalfAIOpenShopAction : InstantActionEvent;
public sealed partial class ReactivateCameraActionEvent : InstantActionEvent;
public sealed partial class UpgradeCamerasActionEvent : InstantActionEvent;
public sealed partial class MachineOverloadActionEvent : EntityTargetActionEvent;
public sealed partial class HostileLockdownActionEvent : InstantActionEvent;
public sealed partial class DoomsDayActionEvent : InstantActionEvent; // Todo
public sealed partial class TurretUpgradeActionEvent : InstantActionEvent; // Todo
public sealed partial class DestoryRCDsActionEvent : InstantActionEvent; // Todo
public sealed partial class BlackoutActionEvent : InstantActionEvent; // Todo
public sealed partial class MakeRoboticFactoryActionEvent : EntityTargetActionEvent; // Todo
