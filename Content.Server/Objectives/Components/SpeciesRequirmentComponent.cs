// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Content.Server.Objectives.Systems;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Requires that the player's species matches a whitelist.
/// </summary>
[RegisterComponent, Access(typeof(SpeciesRequirementSystem))]
public sealed partial class SpeciesRequirementComponent : Component
{
    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public List<ProtoId<SpeciesPrototype>> AllowedSpecies = new();
}
