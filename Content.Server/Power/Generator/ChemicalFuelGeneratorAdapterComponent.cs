// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server.Power.Generator;

/// <summary>
/// This is used for chemical fuel input into generators.
/// </summary>
[RegisterComponent, Access(typeof(GeneratorSystem))]
public sealed partial class ChemicalFuelGeneratorAdapterComponent : Component
{
    /// <summary>
    /// A dictionary relating a reagent to accept as fuel to a value to multiply reagent amount by to get fuel amount.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<ReagentPrototype>, float> Reagents = new();

    /// <summary>
    /// The name of <see cref="Solution"/>.
    /// </summary>
    [DataField("solution")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string SolutionName = "tank";

    /// <summary>
    /// The solution on the <see cref="SolutionContainerManagerComponent"/> to use.
    /// </summary>
    [ViewVariables]
    public Entity<SolutionComponent>? Solution = null;

    /// <summary>
    /// How much reagent (can be fractional) is left in the generator.
    /// Stored in units of <see cref="FixedPoint2.Epsilon"/>.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<ReagentPrototype>, float> FractionalReagents = new();
}