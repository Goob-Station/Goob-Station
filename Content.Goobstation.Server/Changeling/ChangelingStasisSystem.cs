using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.Changeling.Systems;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Polymorph.Systems;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.Polymorph;

namespace Content.Goobstation.Server.Changeling;

public sealed partial class ChangelingStasisSystem : SharedChangelingStasisSystem
{
    [Dependency] private readonly FlammableSystem _flame = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;

    private EntityQuery<FlammableComponent> _flameQuery;
    private EntityQuery<ChangelingIdentityComponent> _lingQuery;
    private EntityQuery<TemperatureComponent> _tempQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingStasisComponent, PolymorphedEvent>(OnPolymorphed);

        _flameQuery = GetEntityQuery<FlammableComponent>();
        _lingQuery = GetEntityQuery<ChangelingIdentityComponent>();
        _tempQuery = GetEntityQuery<TemperatureComponent>();
    }

    private void OnPolymorphed(Entity<ChangelingStasisComponent> ent, ref PolymorphedEvent args)
    {
        if (_lingQuery.TryComp(ent, out var ling)
            && ling.IsInLastResort)
            return;

        _polymorph.CopyPolymorphComponent<ChangelingStasisComponent>(ent, args.NewEntity);
    }

    #region Helper Methods
    protected override void ResetTemperature(Entity<ChangelingStasisComponent> ent)
    {
        if (!_tempQuery.TryComp(ent, out var tempComp))
            return;

        _temperature.ForceChangeTemperature(ent, ent.Comp.IdealTemp, tempComp);
    }

    protected override void ExtinguishFire(Entity<ChangelingStasisComponent> ent)
    {
        if (!_flameQuery.TryComp(ent, out var flameComp))
            return;

        _flame.Extinguish(ent, flameComp);
    }
    #endregion
}
