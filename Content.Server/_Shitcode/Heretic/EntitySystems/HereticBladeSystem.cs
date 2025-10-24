using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Systems;
using Content.Server.Teleportation;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared.Atmos;
using Content.Shared.Teleportation;

namespace Content.Server.Heretic.EntitySystems;

public sealed class HereticBladeSystem : SharedHereticBladeSystem
{
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly TemperatureSystem _temp = default!;
    [Dependency] private readonly TeleportSystem _teleport = default!;

    protected override void ApplyAshBladeEffect(EntityUid target)
    {
        base.ApplyAshBladeEffect(target);

        _flammable.AdjustFireStacks(target, 2.5f, ignite: true);
    }

    protected override void ApplyFleshBladeEffect(EntityUid target)
    {
        base.ApplyFleshBladeEffect(target);

        _blood.TryModifyBleedAmount(target, 1.5f);
    }

    protected override void ApplyVoidBladeEffect(EntityUid target)
    {
        base.ApplyVoidBladeEffect(target);

        if (TryComp<TemperatureComponent>(target, out var temp))
            _temp.ForceChangeTemperature(target, Math.Max(Atmospherics.TCMB, temp.CurrentTemperature - 20f), temp);
    }

    protected override void RandomTeleport(EntityUid user, EntityUid blade, RandomTeleportComponent comp)
    {
        base.RandomTeleport(user, blade, comp);

        _teleport.RandomTeleport(user, comp, false, true);
        QueueDel(blade);
    }
}
