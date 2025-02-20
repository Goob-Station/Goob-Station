using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class FadingTimedDespawnSystem : SharedFadingTimedDespawnSystem
{
    protected override bool CanDelete(EntityUid uid)
    {
        return true;
    }
}
