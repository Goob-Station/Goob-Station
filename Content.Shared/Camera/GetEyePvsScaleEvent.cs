// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;

namespace Content.Shared.Camera;

/// <summary>
///     Raised directed by-ref when <see cref="SharedContentEyeSystem.UpdatePvsScale"/> is called.
///     Should be subscribed to by any systems that want to modify an entity's eye PVS scale,
///     so that they do not override each other. Keep in mind that this should be done serverside;
///     the client may set a new PVS scale, but the server won't provide the data if it isn't done on the server.
/// </summary>
/// <param name="Scale">
///     The total scale to apply.
/// </param>
/// <remarks>
///     Note that in most cases <see cref="Scale"/> should be incremented or decremented by subscribers, not set.
///     Otherwise, any offsets applied by previous subscribing systems will be overridden.
/// </remarks>
[ByRefEvent]
public record struct GetEyePvsScaleEvent(float Scale);

/// <summary>
///     Raised on any equipped and in-hand items that may modify the eye offset.
///     Pockets and suitstorage are excluded.
/// </summary>
[ByRefEvent]
public sealed class GetEyePvsScaleRelayedEvent : EntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; } = ~(SlotFlags.POCKET & SlotFlags.SUITSTORAGE);

    public float Scale;
}