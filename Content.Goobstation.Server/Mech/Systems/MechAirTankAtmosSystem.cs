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

        SubscribeLocalEvent<MechAirTankModuleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<MechAirComponent, EntInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<MechAirComponent, EntRemovedFromContainerMessage>(OnRemoved);
    }

    private void OnMapInit(Entity<MechAirTankModuleComponent> module, ref MapInitEvent args)
    {
        if (module.Comp.Air != null)
            return;

        var air = new GasMixture(module.Comp.Volume) { Temperature = Atmospherics.T20C };
        var moles = Atmospherics.OneAtmosphere * module.Comp.Volume / (Atmospherics.R * Atmospherics.T20C);
        air.AdjustMoles(Gas.Oxygen, moles * 0.21f);
        air.AdjustMoles(Gas.Nitrogen, moles * 0.79f);
        module.Comp.Air = air;
    }

    private void OnInserted(Entity<MechAirComponent> mech, ref EntInsertedIntoContainerMessage args)
    {
        if (!TryComp<MechAirTankModuleComponent>(args.Entity, out var tank))
            return;

        mech.Comp.Air.Volume += tank.Volume;
        if (tank.Air == null)
            return;

        _atmos.Merge(mech.Comp.Air, tank.Air);
        tank.Air = null;
    }

    private void OnRemoved(Entity<MechAirComponent> mech, ref EntRemovedFromContainerMessage args)
    {
        if (!TryComp<MechAirTankModuleComponent>(args.Entity, out var tank))
            return;

        if (mech.Comp.Air.Volume > tank.Volume)
        {
            tank.Air = mech.Comp.Air.RemoveVolume(tank.Volume);
            mech.Comp.Air.Volume -= tank.Volume;
        }
        else
        {
            tank.Air = new GasMixture(tank.Volume) { Temperature = Atmospherics.T20C };
        }
    }
}
