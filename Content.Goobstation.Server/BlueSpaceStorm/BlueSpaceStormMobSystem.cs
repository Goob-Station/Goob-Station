using Content.Goobstation.Common.BlueSpaceStorm;
using Content.Shared.Mobs;

public sealed class BlueSpaceStormMobSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlueSpaceStormPortalMobComponent, MobStateChangedEvent>(OnPortalMobDeath);

    }

    private void OnPortalMobDeath(Entity<BlueSpaceStormPortalMobComponent> mob, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
        {
            return;
        }
        if (!TryComp<BlueSpaceStormPortalComponent>(mob.Comp.LinkedPortal, out var portalComponent))
            return;
        if (!portalComponent.SpawnedMobs.Remove(mob.Owner))
            return;

        if (portalComponent.SpawnedMobs.Count == 0 && !portalComponent.MobsAllDeadEventRaised)
        {
            portalComponent.MobsAllDeadEventRaised = true;
            RaiseLocalEvent(mob.Comp.LinkedPortal, new PortalMobsAllDeathEvent());
        }
    }
}