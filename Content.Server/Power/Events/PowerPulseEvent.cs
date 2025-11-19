// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿namespace Content.Server.Power.Events;

/// <summary>
///     Invoked on a target entity, when it was pulsed with an energy.
///     For instance, interacted with an active stun baton.
/// </summary>
public sealed class PowerPulseEvent : EntityEventArgs
{
    public EntityUid? User;
    public EntityUid? Used;
}
