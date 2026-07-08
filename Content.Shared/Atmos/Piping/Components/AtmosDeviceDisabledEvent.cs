// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Atmos.Piping.Components;

/// <summary>
///     Raised directed on an atmos device when it is enabled.
/// </summary>
[ByRefEvent]
public readonly record struct AtmosDeviceDisabledEvent;