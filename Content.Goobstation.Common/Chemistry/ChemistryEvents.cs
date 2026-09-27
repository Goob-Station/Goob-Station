// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Chemistry;

/// <summary>
/// This event is fired off when a solution reacts.
/// </summary>
[ByRefEvent]
public sealed partial class SolutionReactedEvent : EntityEventArgs;

/// <summary>
/// This event is fired off before a solution reacts.
/// </summary>
[ByRefEvent]
public sealed partial class BeforeSolutionReactEvent : CancellableEntityEventArgs;

/// <summary>
/// Raised after entity already processed all of it's reagent reaction
/// </summary>
[ByRefEvent]
public record struct ReactionEntityDoneEvent();
