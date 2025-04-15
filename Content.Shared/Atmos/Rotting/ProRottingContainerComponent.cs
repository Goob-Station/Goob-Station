// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Atmos.Rotting;

/// <summary>
/// Entities inside this container will rot at a faster pace, e.g. a grave
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ProRottingContainerComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float DecayModifier = 3f;
}
