// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Particles;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Particles;

/// <summary>
/// API for <see cref="Trauma.Client.Particles.ParticleSystem"/>.
/// Use these methods to create and remove particle effects from other systems.
/// </summary>
public sealed partial class ParticleSystem
{
    /// <summary>
    /// Spawns a particle effect at the map position of <paramref name="entity"/>,
    /// optionally attaching it so it follows the entity.
    /// </summary>
    /// <param name="effectId">The prototype ID of the effect to spawn.</param>
    /// <param name="entity">The entity to spawn particles on.</param>
    /// <param name="colorOverride">Optional color tint.</param>
    /// <param name="attach">When <c>true</c> (default), the emitter follows <paramref name="entity"/>.</param>
    /// <returns>The <see cref="ActiveEmitter"/> handle, or <c>null</c> if the effect could not be spawned.</returns>
    public ActiveEmitter? CreateParticle(
        ProtoId<ParticleEffectPrototype> effectId,
        EntityUid entity,
        Color? colorOverride = null,
        bool attach = true)
    {
        var coords = _transform.GetMapCoordinates(entity);
        return SpawnEffect(effectId, coords, attach ? entity : null, colorOverride);
    }

    /// <summary>
    /// Spawns a particle effect at the given map coordinates.
    /// </summary>
    /// <param name="effectId">The prototype ID of the effect to spawn.</param>
    /// <param name="coords">World position to spawn at.</param>
    /// <param name="colorOverride">Optional color tint.</param>
    /// <returns>The <see cref="ActiveEmitter"/> handle, or <c>null</c> if the effect could not be spawned.</returns>
    public ActiveEmitter? CreateParticle(
        ProtoId<ParticleEffectPrototype> effectId,
        MapCoordinates coords,
        Color? colorOverride = null)
    {
        return SpawnEffect(effectId, coords, null, colorOverride);
    }

    /// <summary>
    /// Stops and removes a particle emitter by its <see cref="ActiveEmitter"/> reference. Nullable.
    /// </summary>
    public void RemoveParticle(ActiveEmitter? emitter)
    {
        if (emitter != null)
            StopEffect(emitter);
    }

    /// <summary>
    /// Stops and removes a particle emitter by its numeric handle.
    /// </summary>
    public void RemoveParticle(uint handle)
    {
        StopEffect(handle);
    }
}
