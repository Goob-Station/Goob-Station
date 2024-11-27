using System.Linq;
using Content.Shared._Goobstation.CVars;
using Robust.Shared.Configuration;
using Robust.Shared.Map.Components;

namespace Content.Shared._Goobstation.Silo;

public abstract class SharedSiloSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private bool _siloEnabled;

    public override void Initialize()
    {
        base.Initialize();

        _cfg.OnValueChanged(GoobCVars.SiloEnabled, enabled => _siloEnabled = enabled, true);
    }

    public bool TryGetMaterialAmount(EntityUid machine, string material, out int amount)
    {
        amount = 0;
        var silo = GetSilo(machine);
        if (silo == null)
            return false;

        amount = silo.Value.Comp.Storage.GetValueOrDefault(material, 0);
        return true;
    }

    public bool TryGetTotalMaterialAmount(EntityUid machine, out int amount)
    {
        amount = 0;
        var silo = GetSilo(machine);
        if (silo == null)
            return false;

        amount = silo.Value.Comp.Storage.Values.Sum();
        return true;
    }

    public void DirtySilo(EntityUid machine)
    {
        var silo = GetSilo(machine);
        if (silo == null)
            return;
        Dirty(silo.Value);
    }

    public Entity<SiloComponent>? GetSilo(EntityUid machine)
    {
        if (!_siloEnabled)
            return null;

        var grid = _transform.GetGrid(machine);
        if (grid == null)
            return null;

        var query = AllEntityQuery<SiloComponent, MapGridComponent>();
        while (query.MoveNext(out var ent, out var silo, out _))
        {
            if (grid == ent)
                return (ent, silo);
        }

        return null;
    }
}
