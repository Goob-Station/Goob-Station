// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.BloodCult;

[ByRefEvent]
public sealed class BloodCultRuneDrawnEvent(EntityUid user) : EntityEventArgs
{
    public EntityUid User { get; } = user;
}
