// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Chemistry.EntitySystems;
using Robust.Shared.GameStates;

namespace Content.Shared.Chemistry.Components;

/// <summary>
/// Basically reverse spiking, instead of using the solution-entity on a beaker, you use the beaker on the solution-entity.
/// If there is not enough volume it will stay in the solution-entity rather than spill onto the floor.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(ScoopableSolutionSystem))]
public sealed partial class ScoopableSolutionComponent : Component
{
    /// <summary>
    /// Solution name that can be scooped from.
    /// </summary>
    [DataField]
    public string Solution = "default";

    /// <summary>
    /// If true, when the whole solution is scooped up the entity will be deleted.
    /// </summary>
    [DataField]
    public bool Delete = true;

    /// <summary>
    /// Popup to show the user when scooping.
    /// Passed entities "scooped" and "beaker".
    /// </summary>
    [DataField]
    public LocId Popup = "scoopable-component-popup";
}