// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.Shuttles.Events;

/// <summary>
/// Raised when trying to get a priority tag for docking.
/// </summary>
[ByRefEvent]
public record struct FTLTagEvent(bool Handled, string? Tag);
