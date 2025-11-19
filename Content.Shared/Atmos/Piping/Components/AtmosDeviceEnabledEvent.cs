// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Shared.Atmos.Piping.Components;

/// <summary>
///     Raised directed on an atmos device when it is enabled.
/// </summary>
[ByRefEvent]
public readonly record struct AtmosDeviceEnabledEvent;