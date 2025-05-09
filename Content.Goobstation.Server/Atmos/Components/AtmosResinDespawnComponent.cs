// SPDX-FileCopyrightText: 2025 GoidaBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goidastation.Server.Atmos.EntitySystems;

namespace Content.Goidastation.Server.Atmos.Components;

/// <summary>
/// Assmos - Extinguisher Nozzle
/// </summary>
[RegisterComponent, Access(typeof(AtmosResinDespawnSystem))]
public sealed partial class AtmosResinDespawnComponent : Component
{
}
