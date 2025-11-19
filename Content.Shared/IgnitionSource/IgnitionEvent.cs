// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Shared.IgnitionSource;

/// <summary>
///     Raised in order to toggle the <see cref="IgnitionSourceComponent"/> on an entity on or off
/// </summary>
[ByRefEvent]
public readonly record struct IgnitionEvent(bool Ignite = false);
