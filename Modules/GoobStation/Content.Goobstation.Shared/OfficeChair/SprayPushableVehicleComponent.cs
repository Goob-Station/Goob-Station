// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using System.Numerics;

namespace Content.Goobstation.Shared.OfficeChair;

[RegisterComponent, NetworkedComponent]
public sealed partial class SprayPushableVehicleComponent : Component
{
    /// <summary>
    /// Scale applied to externally requested velocity impulses (spray push).
    /// 1.0 applies the full impulse; lower values reduce its strength.
    /// </summary>
    [DataField]
    public float Multiplier = 0.5f;

    /// <summary>
    /// Default time in seconds over which an enqueued impulse is spread.
    /// </summary>
    [DataField]
    public float ImpulseDuration = 0.5f;

    // Internal shit
    public Vector2 PendingImpulseRemaining;
    public TimeSpan PendingImpulseTimeLeft;
}

[NetSerializable, Serializable]
public sealed partial class SprayUserImpulseEvent : EntityEventArgs
{
    public Vector2 Impulse;

    public SprayUserImpulseEvent(Vector2 impulse)
    {
        Impulse = impulse;
    }
}
