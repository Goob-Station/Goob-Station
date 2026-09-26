using Content.Goobstation.Common.BlueSpaceStorm;
using Content.Shared.Mobs;

public sealed class BlueSpaceStormMobSystem : EntitySystem
{

    [Dependency] private readonly IEntityManager _entity = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlueSpaceStormPortalMobComponent, MobStateChangedEvent>(OnPortalMobDeath);

    }

    private void OnPortalMobDeath(EntityUid uid, BlueSpaceStormPortalMobComponent component, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
        {
            return;
        }
        if (!TryComp<BlueSpaceStormPortalComponent>(component.LinkedPortal, out var portalComponent))
            return;
        portalComponent.SpawnedMobs.Remove(uid);
        if (portalComponent.SpawnedMobs.Count < 1)
        {
            RaiseLocalEvent(component.LinkedPortal, new PortalMobsAllDeathEvent());
        }
    }
}