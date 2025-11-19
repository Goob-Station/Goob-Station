// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Content.Shared.Atmos;

namespace Content.Server.Xenoarchaeology.Artifact.XAE.Components;

/// <summary>
/// XenoArtifact effect that creates gas in atmosphere.
/// </summary>
[RegisterComponent, Access(typeof(XAECreateGasSystem))]
public sealed partial class XAECreateGasComponent : Component
{
    /// <summary>
    /// The gases and how many moles will be created of each.
    /// </summary>
    [DataField]
    public Dictionary<Gas, float> Gases = new();
}
