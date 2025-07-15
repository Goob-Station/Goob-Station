using Content.Server.Temperature.Components;

namespace Content.Pirate.Server.Traits.HeatResistant;

public sealed class TempResistantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TempResistantComponent, MapInitEvent>(OnInit);
    }

    private void OnInit(Entity<TempResistantComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<TemperatureComponent>(ent.Owner, out var temperature)) return;
        temperature.HeatDamageThreshold *= ent.Comp.HeatModifier;
        temperature.ColdDamageThreshold *= ent.Comp.ColdModifier;
    }
}
