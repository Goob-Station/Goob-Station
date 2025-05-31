// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <marcus2008stoke@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Temperature.Components;

/// <summary>
///     Used to ensure that LowTempImmunityComponent is not overriden (when it is made eventually)
/// </summary>
[RegisterComponent]
public sealed partial class SpecialLowTempImmunityComponent : Component;
