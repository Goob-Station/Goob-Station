// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Procedural.Components;

/// <summary>
/// Component that is used for granting components to entities that enter the ruin.
/// </summary>
[RegisterComponent]
public sealed partial class LavalandGridGrantComponent : Component
{
    /// <summary>
    /// List of components to grant to entities that enter the ruin.
    /// </summary>
    [DataField]
    public ComponentRegistry ComponentsToGrant = new();
}