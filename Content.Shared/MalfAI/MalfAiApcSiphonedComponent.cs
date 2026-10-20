// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.MalfAI;

/// <summary>
/// Component indicating this APC is currently siphoned by a Malf AI.
/// Siphoned APCs are completely disabled and cannot be interacted with.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class MalfAiApcSiphonedComponent : Component
{
    /// <summary>
    /// The time when the siphon expires and the APC can be siphoned again.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan SiphonEndTime;

    /// <summary>
    /// Whether the APC was already emagged before being siphoned.
    /// </summary>
    [DataField]
    public bool WasEmagged;

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
