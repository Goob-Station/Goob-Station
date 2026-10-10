using Content.Goobstation.Common.BluespaceStorm;
using Content.Shared.Mobs;

namespace Content.Goobstation.Server.BluespaceStorm;

public sealed class BluespaceStormMobSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BluespaceStormPortalMobComponent, MobStateChangedEvent>(OnPortalMobDeath);

    }

    private void OnPortalMobDeath(Entity<BluespaceStormPortalMobComponent> mob, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
        {
            return;
        }
        if (!TryComp<BluespaceStormPortalComponent>(mob.Comp.LinkedPortal, out var portalComponent))
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