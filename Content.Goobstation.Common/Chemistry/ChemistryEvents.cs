// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
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
