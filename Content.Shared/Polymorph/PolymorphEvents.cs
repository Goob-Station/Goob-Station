// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Polymorph;

/// <summary>
/// Raised locally on an entity when it polymorphs into another entity
/// </summary>
/// <param name="OldEntity">EntityUid of the entity before the polymorph</param>
/// <param name="NewEntity">EntityUid of the entity after the polymorph</param>
/// <param name="IsRevert">Whether this polymorph event was a revert back to the original entity</param>
[ByRefEvent]
public record struct PolymorphedEvent(EntityUid OldEntity, EntityUid NewEntity, bool IsRevert);