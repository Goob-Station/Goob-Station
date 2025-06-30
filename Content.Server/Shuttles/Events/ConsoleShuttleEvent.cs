// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Shuttles.Components;

namespace Content.Server.Shuttles.Events;

/// <summary>
/// Raised on a <see cref="ShuttleConsoleComponent"/> when it's trying to get its shuttle console to pilot.
/// </summary>
[ByRefEvent]
public struct ConsoleShuttleEvent
{
    /// <summary>
    /// Console that we proxy into.
    /// </summary>
    public EntityUid? Console;
}