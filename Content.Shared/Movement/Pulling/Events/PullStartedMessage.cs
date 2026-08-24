// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Movement.Pulling.Events;

/// <summary>
/// Event raised directed BOTH at the puller and pulled entity when a pull starts.
/// </summary>
public sealed class PullStartedMessage(EntityUid pullerUid, EntityUid pullableUid) : PullMessage(pullerUid, pullableUid);