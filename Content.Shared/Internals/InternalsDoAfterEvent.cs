// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Alert;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Internals;

[Serializable, NetSerializable]
public sealed partial class InternalsDoAfterEvent : SimpleDoAfterEvent
{
}

public sealed partial class ToggleInternalsAlertEvent : BaseAlertEvent;
