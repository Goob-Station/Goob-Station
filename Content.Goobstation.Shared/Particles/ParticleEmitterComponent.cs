// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Particles;

/// <summary>
/// Spawns a particle effect on this entity when it initializes.
/// </summary>
[RegisterComponent]
public sealed partial class ParticleEmitterComponent : Component
{
    /// <summary>
    /// The particle effect to emit.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<ParticleEffectPrototype> Effect;

    /// <summary>
    /// Optional color tint applied to every particle of this emitter.
    /// Multiplied on top of the prototype colors; null leaves colors unchanged.
    /// </summary>
    [DataField]
    public Color? ColorOverride;

    /// <summary>
    /// Starting intensity multiplier (0–1+). Scales emission rate and particle size.
    /// 1.0 = normal.
    /// </summary>
    [DataField]
    public float Intensity = 1f;
}
