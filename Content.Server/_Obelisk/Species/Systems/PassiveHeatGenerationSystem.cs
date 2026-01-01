using Content.Server._Obelisk.Species.Components;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.Mobs.Components;

namespace Content.Server._Obelisk.Species.Systems;

public sealed class PassiveHeatGenerationSystem : EntitySystem
{
    [Dependency] private readonly TemperatureSystem _temperature = default!;

    // In seconds
    private const float UpdateInterval = 1;
    // How much time has passed since the last update in seconds
    private float _accumulator;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _accumulator += frameTime;
        if (_accumulator <= UpdateInterval)
            return;

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

            _temperature.ChangeHeat(uid, watts * _accumulator, passiveHeatComp.IgnoreHeatResistance, tempComp);
        }

        _accumulator = 0;
    }
}
