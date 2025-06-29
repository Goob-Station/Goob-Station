// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Atmos.EntitySystems;

namespace Content.Goobstation.Server.Atmos.Components;

/// <summary>
/// Assmos - Extinguisher Nozzle
/// </summary>
[RegisterComponent, Access(typeof(AtmosResinDespawnSystem))]
public sealed partial class AtmosResinDespawnComponent : Component
{
}
