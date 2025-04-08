// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Burial.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShovelComponent : Component
{
    /// <summary>
    /// The speed modifier for how fast this shovel will dig.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float SpeedModifier = 1f;
}