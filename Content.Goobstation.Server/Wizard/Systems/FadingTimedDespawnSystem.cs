using Content.Goobstation.Shared.Wizard.FadingTimedDespawn;

namespace Content.Goobstation.Server.Wizard.Systems;

public sealed class FadingTimedDespawnSystem : SharedFadingTimedDespawnSystem
{
    protected override bool CanDelete(EntityUid uid)
    {
        return true;
    }
}
