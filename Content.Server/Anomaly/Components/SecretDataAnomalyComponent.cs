// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Anomaly.Effects;

namespace Content.Server.Anomaly.Components;

/// <summary>
/// Hides some information about the anomaly when scanning it
/// </summary>
[RegisterComponent, Access(typeof(SecretDataAnomalySystem), typeof(AnomalySystem))]
public sealed partial class SecretDataAnomalyComponent : Component
{
    /// <summary>
    /// Minimum hidden data elements on MapInit
    /// </summary>
    [DataField]
    public int RandomStartSecretMin = 0;

    /// <summary>
    /// Maximum hidden data elements on MapInit
    /// </summary>
    [DataField]
    public int RandomStartSecretMax = 0;

    /// <summary>
    /// Current secret data
    /// </summary>
    [DataField]
    public List<AnomalySecretData> Secret = new();
}

/// <summary>
/// Enum with secret data field variants
/// </summary>
[Serializable]
public enum AnomalySecretData : byte
{
    Severity,
    Stability,
    OutputPoint,
    ParticleDanger,
    ParticleUnstable,
    ParticleContainment,
    ParticleTransformation,
    Behavior,
    Default
}