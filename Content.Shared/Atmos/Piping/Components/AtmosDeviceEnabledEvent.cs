// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Atmos.Piping.Components;

/// <summary>
/// <para>Raised directed on an AtmosDeviceComponent when it has been added to
/// a GridAtmosphereComponent.
/// This can occur when it has been requested manually or when the device has been anchored.</para>
///
/// <para>You can use this to cache specific information about the grid the device is on,
/// though be careful with caching references to objects that can be removed or changed at any time
/// (such as <see cref="GasMixture"/>s).</para>
/// </summary>
[ByRefEvent]
public readonly record struct AtmosDeviceEnabledEvent;
