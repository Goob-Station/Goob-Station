// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Identity;

[ByRefEvent]
public record struct GetIdentityRepresentationEntityEvent(EntityUid? Uid = null);
