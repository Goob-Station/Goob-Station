using Content.Goobstation.Shared.Mech.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Mech.Components;
using Content.Shared.Atmos;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Mech.Systems;

public sealed partial class MechAirTankAtmosSystem : EntitySystem
{
    [Dependency] private AtmosphereSystem _atmos = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MechAirComponent, EntInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<MechAirComponent, EntRemovedFromContainerMessage>(OnRemoved);
    }

    private void OnInserted(Entity<MechAirComponent> mech, ref EntInsertedIntoContainerMessage args)
    {
        if (!TryComp<MechAirTankModuleComponent>(args.Entity, out var tank))
            return;

        var volume = tank.Air.Volume;
        mech.Comp.Air.Volume += volume;
        _atmos.Merge(mech.Comp.Air, tank.Air);
        tank.Air = new GasMixture(volume) { Temperature = tank.Air.Temperature };
    }

    private void OnRemoved(Entity<MechAirComponent> mech, ref EntRemovedFromContainerMessage args)
    {
        if (!TryComp<MechAirTankModuleComponent>(args.Entity, out var tank))
            return;

        var volume = tank.Air.Volume;
        if (mech.Comp.Air.Volume > volume)
        {
            tank.Air = mech.Comp.Air.RemoveVolume(volume);
            tank.Air.Volume = volume;
            mech.Comp.Air.Volume -= volume;
        }
        else
        {
            tank.Air = new GasMixture(volume) { Temperature = Atmospherics.T20C };
        }
    }
}
