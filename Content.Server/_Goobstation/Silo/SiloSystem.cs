using System.Linq;
using Content.Server.Station.Components;
using Content.Server.Station.Events;
using Content.Shared._Goobstation.Silo;

namespace Content.Server._Goobstation.Silo;

public sealed class SiloSystem : SharedSiloSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationSiloComponent, StationPostInitEvent>(OnPostInit);
    }

    private void OnPostInit(Entity<StationSiloComponent> ent, ref StationPostInitEvent args)
    {
        var grids = args.Station.Comp.Grids;
        foreach (var grid in grids.Where(HasComp<BecomesStationComponent>))
        {
            EnsureComp<SiloComponent>(grid);
        }
    }
}
