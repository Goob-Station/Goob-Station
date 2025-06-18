// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Cyberdeck.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CyberdeckSourceComponent : Component
{
    /// <summary>
    /// Timer that updates charges based on AutoRechargeComponent.
    /// </summary>
    [ViewVariables]
    public float? Accumulator;
}
