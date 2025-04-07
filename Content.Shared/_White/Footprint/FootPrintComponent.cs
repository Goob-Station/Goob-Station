// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._White.FootPrint;

/// <summary>
/// This is used for marking footsteps, handling footprint drawing.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FootPrintComponent : Component
{
    /// <summary>
    /// Owner (with <see cref="FootPrintsComponent"/>) of a print (this component).
    /// </summary>
    [AutoNetworkedField]
    public EntityUid PrintOwner;

    [DataField]
    public string SolutionName = "step";

    [ViewVariables]
    public Entity<SolutionComponent>? Solution;
}