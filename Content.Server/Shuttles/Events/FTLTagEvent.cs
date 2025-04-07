// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Shuttles.Events;

/// <summary>
/// Raised when trying to get a priority tag for docking.
/// </summary>
[ByRefEvent]
public record struct FTLTagEvent(bool Handled, string? Tag);