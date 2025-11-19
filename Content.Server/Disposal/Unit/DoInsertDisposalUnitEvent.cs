// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.Disposal.Unit
{
    public record DoInsertDisposalUnitEvent(EntityUid? User, EntityUid ToInsert, EntityUid Unit);
}
