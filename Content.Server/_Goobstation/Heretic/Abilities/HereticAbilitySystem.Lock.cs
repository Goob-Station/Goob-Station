using Content.Shared.Heretic;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem : EntitySystem
{
    private void SubscribeLock()
    {
        SubscribeLocalEvent<HereticComponent, EventHereticBulglarFinesse>(OnBulglarFinesse);
        SubscribeLocalEvent<HereticComponent, EventHereticLastRefugee>(OnLastRefugee);
        // add eldritch id here

        SubscribeLocalEvent<HereticComponent, HereticAscensionLockEvent>(OnAscensionLock);
    }

    private void OnBulglarFinesse(Entity<HereticComponent> ent, ref EventHereticBulglarFinesse args)
    {

    }
    private void OnLastRefugee(Entity<HereticComponent> ent, ref EventHereticLastRefugee args)
    {

    }

    private void OnAscensionLock(Entity<HereticComponent> ent, ref HereticAscensionLockEvent args)
    {

    }
}
