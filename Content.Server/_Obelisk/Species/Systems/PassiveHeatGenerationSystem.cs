using Content.Server._Obelisk.Species.Components;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.Mobs.Components;
using Robust.Shared.Timing;

namespace Content.Server._Obelisk.Species.Systems;

public sealed class PassiveHeatGenerationSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;

    private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    private TimeSpan _lastUpdate;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_lastUpdate + UpdateInterval >= _timing.CurTime)
            return;

        var deltaTime = (float) (_timing.CurTime - _lastUpdate).TotalSeconds;
        _lastUpdate = _timing.CurTime;

        var query = EntityQueryEnumerator<PassiveHeatGenerationComponent, TemperatureComponent>();

        while (query.MoveNext(out var uid, out var passiveHeatComp, out var tempComp))
        {
            // If your too cold or hot don't update.
            var currentTemp = tempComp.CurrentTemperature;
            if (currentTemp > passiveHeatComp.MaximumTemperature || currentTemp < passiveHeatComp.MinimumTemperature)
                continue;

            // Modify the heat by the mob state modifiers (if they exist)
            var watts = passiveHeatComp.Watts;
            if (passiveHeatComp.MobStateModifier != null && TryComp<MobStateComponent>(uid, out var mobStateComp))
            {
                var currentState = mobStateComp.CurrentState;
                if (passiveHeatComp.MobStateModifier.TryGetValue(currentState, out var modifier))
                    watts *= modifier;
            }

            _temperature.ChangeHeat(uid, watts * deltaTime, passiveHeatComp.IgnoreHeatResistance, tempComp);
        }
    }
}
