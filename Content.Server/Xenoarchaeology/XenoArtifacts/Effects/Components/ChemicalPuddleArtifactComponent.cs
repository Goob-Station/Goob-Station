// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;

/// <summary>
/// This is used for an artifact that creates a puddle of
/// random chemicals upon being triggered.
/// </summary>
[RegisterComponent, Access(typeof(ChemicalPuddleArtifactSystem))]
public sealed partial class ChemicalPuddleArtifactComponent : Component
{
    /// <summary>
    /// The solution where all the chemicals are stored
    /// </summary>
    [DataField("chemicalSolution", required: true), ViewVariables(VVAccess.ReadWrite)]
    public Solution ChemicalSolution = default!;

    /// <summary>
    /// The different chemicals that can be spawned by this effect
    /// </summary>
    [DataField("possibleChemicals", required: true, customTypeSerializer: typeof(PrototypeIdListSerializer<ReagentPrototype>))]
    public List<string> PossibleChemicals = default!;

    /// <summary>
    /// The number of chemicals in the puddle
    /// </summary>
    [DataField("chemAmount")]
    public int ChemAmount = 3;
}