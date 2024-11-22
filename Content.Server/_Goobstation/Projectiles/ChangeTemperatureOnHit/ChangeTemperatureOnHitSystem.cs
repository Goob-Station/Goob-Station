using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.Atmos;
using Content.Shared.Projectiles;

namespace Content.Server._Goobstation.Projectiles.ChangeTemperatureOnHit;

public sealed class ChangeTemperatureOnHitSystem : EntitySystem
{
    [Dependency] private readonly TemperatureSystem _temperature = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangeTemperatureOnHitComponent, ProjectileHitEvent>(OnHit);
    }

    private void OnHit(Entity<ChangeTemperatureOnHitComponent> ent, ref ProjectileHitEvent args)
    {
        if (!TryComp(args.Target, out TemperatureComponent? temperature))
            return;

        var temp = temperature.CurrentTemperature + ent.Comp.Delta;
        temp = Math.Max(Atmospherics.TCMB, temp);
        _temperature.ForceChangeTemperature(args.Target, temp, temperature);
    }
}
