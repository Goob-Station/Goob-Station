using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Systems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Teleportation;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Teleportation;
using Robust.Shared.Prototypes;

namespace Content.Server.Heretic.EntitySystems;

public sealed class HereticBladeSystem : SharedHereticBladeSystem
{
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly TemperatureSystem _temp = default!;
    [Dependency] private readonly TeleportSystem _teleport = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _sol = default!;
    [Dependency] private readonly PuddleSystem _puddle = default!;

    [Dependency] private readonly IPrototypeManager _proto = default!;

    protected override void ApplyAshBladeEffect(EntityUid target)
    {
        base.ApplyAshBladeEffect(target);

        _flammable.AdjustFireStacks(target, 2.5f, ignite: true);
    }

    protected override void ApplyFleshBladeEffect(EntityUid target)
    {
        base.ApplyFleshBladeEffect(target);

        if (!TryComp(target, out BloodstreamComponent? bloodStream))
            return;

        _blood.TryModifyBleedAmount((target, bloodStream), 2f);

        if (!_sol.ResolveSolution(target,
                bloodStream.BloodSolutionName,
                ref bloodStream.BloodSolution,
                out var bloodSolution))
            return;

        _puddle.TrySpillAt(target, bloodSolution.SplitSolution(20), out _);
    }

    protected override void ApplyVoidBladeEffect(EntityUid target)
    {
        base.ApplyVoidBladeEffect(target);

        if (TryComp<TemperatureComponent>(target, out var temp))
            _temp.ForceChangeTemperature(target, temp.CurrentTemperature - 5f, temp);
    }

    protected override void RandomTeleport(EntityUid user, EntityUid blade, RandomTeleportComponent comp)
    {
        base.RandomTeleport(user, blade, comp);

        _teleport.RandomTeleport(user, comp, false);
        QueueDel(blade);
    }
}
