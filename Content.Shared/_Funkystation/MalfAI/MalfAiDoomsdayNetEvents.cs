// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.MalfAI;

/// <summary>
/// Networked event sent to clients to start the doomsday ripple visual effect.
/// </summary>
[Serializable, NetSerializable]
public sealed class MalfAiDoomsdayRippleStartedEvent : EntityEventArgs
{
    public MapId MapId;
    public Vector2 OriginWorld;
    public double ServerStartSeconds;
    public float Duration;
    public float MaxRadiusTiles;
    public bool CenterFlash;

    public MalfAiDoomsdayRippleStartedEvent(
        MapId mapId,
        Vector2 originWorld,
        double serverStartSeconds,
        float duration,
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
