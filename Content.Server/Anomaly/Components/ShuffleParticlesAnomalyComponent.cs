// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Anomaly.Effects;

namespace Content.Server.Anomaly.Components;

/// <summary>
/// Shuffle Particle types in some situations
/// </summary>
[RegisterComponent, Access(typeof(ShuffleParticlesAnomalySystem))]
public sealed partial class ShuffleParticlesAnomalyComponent : Component
{
    /// <summary>
    /// Prob() chance to randomize particle types after Anomaly pulation
    /// </summary>
    [DataField]
    public bool ShuffleOnPulse = false;

    /// <summary>
    /// Prob() chance to randomize particle types after APE or CHIMP projectile
    /// </summary>
    [DataField]
    public bool ShuffleOnParticleHit = false;

    /// <summary>
    /// Chance to random particles
    /// </summary>
    [DataField]
    public float Prob = 0.5f;
}