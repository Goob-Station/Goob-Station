// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.MalfAI.Components;

/// <summary>
/// Component indicating this APC is currently siphoned by a Malf AI.
/// Siphoned APCs are completely disabled and cannot be interacted with.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfAiApcSiphonedComponent : Component
{
    /// <summary>
    /// How long the APC remains siphoned before returning to normal.
    /// </summary>
    [DataField]
    public TimeSpan SiphonDuration = TimeSpan.FromMinutes(1);

    /// <summary>
    /// The original state of the APC's main breaker before siphoning.
    /// </summary>
    [DataField]
    public bool OriginalBreakerState = true;
}

/// <summary>
/// Event sent to an APC to start the siphon process.
/// </summary>
[ByRefEvent]
public readonly record struct ApcStartSiphonEvent(EntityUid SiphonedBy);

/// <summary>
/// Event raised when an APC's siphon expires and it should restore to normal.
/// </summary>
[ByRefEvent]
public readonly record struct ApcSiphonExpiredEvent;
