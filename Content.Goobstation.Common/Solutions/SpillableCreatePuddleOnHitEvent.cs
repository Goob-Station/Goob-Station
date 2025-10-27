// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Map;

namespace Content.Goobstation.Common.Solutions;

[ByRefEvent]
public readonly record struct SpillableCreatePuddleOnHitEvent(
    EntityUid User,
    EntityCoordinates Coords,
    float Amount);
