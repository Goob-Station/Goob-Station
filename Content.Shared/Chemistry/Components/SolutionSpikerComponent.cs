// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Chemistry.EntitySystems;
using Robust.Shared.GameStates;

namespace Content.Shared.Chemistry.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SolutionSpikerSystem))]
public sealed partial class SolutionSpikerComponent : Component
{
    /// <summary>
    ///     The source solution to take the reagents from in order
    ///     to spike the other solution container.
    /// </summary>
    [DataField(required: true)]
    public string SourceSolution = string.Empty;

    /// <summary>
    ///     If spiking with this entity should ignore empty containers or not.
    /// </summary>
    [DataField]
    public bool IgnoreEmpty;

    /// <summary>
    /// If true, the entity is deleted after spiking.
    /// This is almost certainly what you want.
    /// </summary>
    [DataField]
    public bool Delete = true;

    /// <summary>
    ///     What should pop up when spiking with this entity.
    /// </summary>
    [DataField]
    public LocId Popup = "spike-solution-generic";

    /// <summary>
    ///     What should pop up when spiking fails because the container was empty.
    /// </summary>
    [DataField]
    public LocId PopupEmpty = "spike-solution-empty-generic";
}