// SPDX-License-Identifier: AGPL-3.0-or-later


using Content.Goobstation.Shared.Wizard.Systems;

namespace Content.Goobstation.Server.Wizard.Systems;

public sealed class FadingTimedDespawnSystem : SharedFadingTimedDespawnSystem
{
    protected override bool CanDelete(EntityUid uid)
    {
        return true;
    }
}