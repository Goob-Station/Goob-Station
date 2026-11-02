using Content.Goobstation.Shared.ModSuits.Components;
using Content.Shared.Temperature.Components;
using Content.Shared.Temperature.Systems;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModThermalRegulatorModuleSystem : EntitySystem
{
    [Dependency] private SharedTemperatureSystem _temperature = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ModThermalRegulatorModuleComponent, ModModuleComponent>();
        while (query.MoveNext(out _, out var regulator, out var module))
        {
            if (!module.Active
                || !TryComp<ModSuitComponent>(module.Suit, out var suit)
                || suit.Wearer is not { } wearer
                || !TryComp<TemperatureComponent>(wearer, out var temperature))
                continue;

            var difference = regulator.TargetTemperature - temperature.CurrentTemperature;
            if (MathF.Abs(difference) < 0.01f)
                continue;

            var step = MathF.Sign(difference) * MathF.Min(MathF.Abs(difference), regulator.Rate * frameTime);
            var heat = step * _temperature.GetHeatCapacity(wearer, temperature);
            _temperature.ChangeHeat(wearer, heat, true, temperature);
        }
    }
}
