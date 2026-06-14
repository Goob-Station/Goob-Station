// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Map;
using System.Numerics;

namespace Content.Server._Funkystation.MalfAI.Doomsday;

/// <summary>
/// Tracks pending doomsday ripple damage and round-end scheduling after the protocol fires.
/// </summary>
[RegisterComponent]
public sealed partial class MalfAiDoomsdayRipplePendingComponent : Component
{
    [DataField]
    public MapId TargetMapId;

    [DataField]
    public Vector2 OriginPos;

    /// <summary>When to deal lethal radiation damage to everything on the map.</summary>
    [DataField]
    public TimeSpan DamageTime;

    /// <summary>When to call EndRound after the damage phase.</summary>
    [DataField]
    public TimeSpan RoundEndTime;

    [DataField]
    public bool DamageDealt;
}
