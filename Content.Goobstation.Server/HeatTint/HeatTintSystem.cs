using Content.Goobstation.Shared.HeatTint;
using Content.Server.Temperature.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Temperature;

namespace Content.Goobstation.Server.HeatTint;

public sealed class HeatTintSystem : SharedHeatTintSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeatTintComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<HeatTintComponent, OnTemperatureChangeEvent>(OnTemperatureChange);
        SubscribeLocalEvent<HeatTintComponent, SolutionContainerChangedEvent>(OnSolutionChanged);
    }

    private void OnMapInit(Entity<HeatTintComponent> ent, ref MapInitEvent args)
    {
        if (TryComp<TemperatureComponent>(ent, out var temp))
            _appearance.SetData(ent, HeatTintVisuals.Temperature, temp.CurrentTemperature);
    }

    private void OnTemperatureChange(Entity<HeatTintComponent> ent, ref OnTemperatureChangeEvent args)
    {
        _appearance.SetData(ent, HeatTintVisuals.Temperature, args.CurrentTemperature);
    }

    private void OnSolutionChanged(Entity<HeatTintComponent> ent, ref SolutionContainerChangedEvent args)
    {
        _appearance.SetData(ent, HeatTintVisuals.Temperature, args.Solution.Temperature);
    }
}
