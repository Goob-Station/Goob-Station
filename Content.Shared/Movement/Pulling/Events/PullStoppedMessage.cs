// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Movement.Pulling.Events;

/// <summary>
/// Event raised directed BOTH at the puller and pulled entity when a pull stops.
/// </summary>
public sealed class PullStoppedMessage(EntityUid pullerUid, EntityUid pulledUid) : PullMessage(pullerUid, pulledUid);
