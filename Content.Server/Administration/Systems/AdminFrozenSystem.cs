// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 no <165581243+pissdemon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Administration;

namespace Content.Server.Administration.Systems;

public sealed class AdminFrozenSystem : SharedAdminFrozenSystem
{
    /// <summary>
    /// Freezes and mutes the given entity.
    /// </summary>
    public void FreezeAndMute(EntityUid uid)
    {
        var comp = EnsureComp<AdminFrozenComponent>(uid);
        comp.Muted = true;
        Dirty(uid, comp);
    }
}