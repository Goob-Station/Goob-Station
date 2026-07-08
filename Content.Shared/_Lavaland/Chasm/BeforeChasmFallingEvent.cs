// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Lavaland.Chasm;

[ByRefEvent]
public record struct BeforeChasmFallingEvent(EntityUid Entity, bool Cancelled = false);