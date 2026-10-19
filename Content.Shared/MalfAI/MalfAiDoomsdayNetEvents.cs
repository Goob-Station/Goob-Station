// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using System;
using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Vector2 = System.Numerics.Vector2;

namespace Content.Shared.MalfAI;

/// <summary>
/// Broadcast by the server when the Malf AI Doomsday ripple begins,
/// so clients can render a synced visual effect.
/// Time is provided in server time seconds to allow client-side sync.
/// </summary>
[Serializable, NetSerializable]
public sealed class MalfAiDoomsdayRippleStartedEvent : EntityEventArgs
{
    public MapId MapId { get; }
    public Vector2 OriginWorld { get; }
    public double ServerStartSeconds { get; }
    public TimeSpan Duration { get; }
    public float MaxRadiusTiles { get; }
    public bool CenterFlash { get; }

    public MalfAiDoomsdayRippleStartedEvent(
        MapId mapId,
        Vector2 originWorld,
        double serverStartSeconds,
        TimeSpan duration,
        float maxRadiusTiles,
        bool centerFlash)
    {
        MapId = mapId;
        OriginWorld = originWorld;
        ServerStartSeconds = serverStartSeconds;
        Duration = duration;
        MaxRadiusTiles = maxRadiusTiles;
        CenterFlash = centerFlash;
    }
}
