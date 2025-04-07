// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Singularity.Components;

namespace Content.Client.ParticleAccelerator;

[RegisterComponent]
[Access(typeof(ParticleAcceleratorPartVisualizerSystem))]
public sealed partial class ParticleAcceleratorPartVisualsComponent : Component
{
    [DataField("stateBase", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public string StateBase = default!;

    [DataField("stateSuffixes")]
    [ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<ParticleAcceleratorVisualState, string> StatesSuffixes = new()
    {
        {ParticleAcceleratorVisualState.Powered, "p"},
        {ParticleAcceleratorVisualState.Level0, "p0"},
        {ParticleAcceleratorVisualState.Level1, "p1"},
        {ParticleAcceleratorVisualState.Level2, "p2"},
        {ParticleAcceleratorVisualState.Level3, "p3"},
    };
}