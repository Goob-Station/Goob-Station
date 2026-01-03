using Content.Shared.Heretic;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem
{
    protected virtual void SubscribeLock()
    {
        SubscribeLocalEvent<HereticComponent, EventHereticLastRefugee>(OnLastRefugee);

        SubscribeLocalEvent<HereticComponent, HereticAscensionLockEvent>(OnAscensionLock);
    }

    private void OnLastRefugee(Entity<HereticComponent> ent, ref EventHereticLastRefugee args)
    {
    }

    private void OnAscensionLock(Entity<HereticComponent> ent, ref HereticAscensionLockEvent args)
    {
    }
}
